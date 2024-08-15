shader_type canvas_item;
render_mode unshaded;

uniform bool environment = false;
uniform vec3 GroundColor : source_color;
uniform vec3 SkyColorHorizon : source_color;
uniform vec3 SkyColorZenith : source_color;
uniform vec3 SunLightDirection;
uniform float SunFocus;
uniform float SunIntensity;


uniform int RaysPerPixel = 1;
uniform int MaxBounces = 2;
//uniform vec3 CameraPosition = vec3(0,0,0);
//uniform vec3 CameraRotation = vec3(0,0,0);
uniform float NearPlane = 0.1;
uniform float FarPlane = 8;
uniform float FOV = 2;
uniform float AspectRatio = 1;
uniform mat4 ViewMatrix;
uniform mat4 InvViewMatrix;
uniform sampler2D testImage;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;
uniform float PreviousFrameWeight;

struct Sphere {
	vec3 position;
	float radius;
	vec3 color;
	float roughness;
	vec3 emissionColor;
	float specular;
	float emissionStrength;
};

struct Ray {
	vec3 origin;
	vec3 dir;
};

struct HitInfo {
	bool hit;
	float dst;
	vec3 hitPoint;
	vec3 normal;
	Sphere sphere;
};

const int Objects = 5;

uniform vec3[Objects] points;
uniform vec3[Objects] colors : source_color;
uniform float[Objects] radii;
uniform float[Objects] roughness;
uniform vec3[Objects] emissionColor : source_color;
uniform float[Objects] emisionStrength;

varying float State;

HitInfo SphereCheckCollision(Ray ray, Sphere sphere) {
	HitInfo hitinfo = HitInfo(false, 0.0, vec3(0,0,0), vec3(0,0,0), sphere);
	vec3 offsetOrigin = ray.origin - sphere.position;

	float a = dot(ray.dir, ray.dir);
	float b = 2.0 * dot(offsetOrigin, ray.dir);
	float c = dot(offsetOrigin, offsetOrigin) - sphere.radius * sphere.radius;

	float discriminant = b * b - 4.0 * a * c;

	if (discriminant >= 0.0) {
		float dst = (-b - sqrt(discriminant)) / (2.0 * a);

		if (dst >= 0.0) {
			hitinfo.hit = true;
			hitinfo.dst = dst;
			hitinfo.hitPoint = ray.origin + ray.dir * dst;
			hitinfo.normal = normalize(hitinfo.hitPoint - sphere.position);
		}
	}
	return hitinfo;
}

HitInfo CalculateCollision(Ray ray, Sphere[Objects] spheres) {
	HitInfo closestHit;
	closestHit.hit = false;
	closestHit.dst = pow(10,5);
	for (int k = 0; k < Objects; k++) {
		HitInfo hi = SphereCheckCollision(ray, spheres[k]);
		if (hi.hit) {
			closestHit.hit = true;
			if (hi.dst < closestHit.dst) {
				closestHit.dst = hi.dst;
				closestHit.sphere = hi.sphere;
				closestHit.normal = hi.normal;
				closestHit.hitPoint = hi.hitPoint;
				closestHit.sphere = hi.sphere;
			}
		}
	}

	return closestHit;
}

float noise(vec2 uv) {
	return fract(sin(dot(uv, vec2(12.25, 10.356))* 23.12 + State) * 5054.53);
	//return fract(sin(dot(uv, vec2(12.25, 10.356 + mod(TIME * 14.23, 1.0)))* 23.12) * 5054.53);
}

float RandomNormalDistribution(inout float prev) {
	prev = noise(vec2(prev, State * 1.1));
	float theta = 2.0 * PI * prev;
	float rho = sqrt(-2.0 * log(noise(vec2(prev,noise(vec2(prev, theta))))));
	return rho * cos(theta);
}

vec3 RandomDirection(float rng) {
	float seed = State;
	float x = RandomNormalDistribution(seed);
	seed = seed * 2.303;
	float y = RandomNormalDistribution(seed);
	seed = seed * 2.343;
	float z = RandomNormalDistribution(seed);
	return normalize(vec3(x,y,z));
}

vec3 GetReflected(vec3 incoming, HitInfo hitInfo) {
	vec3 d = normalize(incoming);
	vec3 n = normalize(hitInfo.normal);
	float prod = dot(d, n);
	vec3 outgoing = normalize(d - n * (2.0 * prod));
	//return outgoing * sign(dot(normalize(d), n));
	return outgoing;
}

vec3 DiffuseReflected(vec3 incoming) {
	vec3 direction = RandomDirection(State);
	return direction * sign(dot(normalize(incoming), direction));
}

vec3 lerp(vec3 st, vec3 end, float t) {
	t = clamp(t, 0, 1);
	return vec3((st.x * (1.0-t) + end.x * t),(st.y * (1.0-t) + end.y * t),(st.z * (1.0-t) + end.z * t));
}

vec3 GetEnvironmentLight(Ray ray) {
	if (environment) {
		float skyGradientT = pow(smoothstep(0, 0.4, ray.dir.y), 0.35);
		vec3 skyGradient = lerp(SkyColorHorizon, SkyColorZenith, skyGradientT);
		float sun = pow(max(0, dot(ray.dir, -SunLightDirection)), SunFocus) * SunIntensity;

		float groundToSkyT = smoothstep(-0.6, 0.01, ray.dir.y);
		float sunMask = groundToSkyT >= 1.0 ? 1.0 : 0.0;
		return lerp(GroundColor, skyGradient,  groundToSkyT) + sun * sunMask;
	}
	return vec3(0);
}


vec3 Trace(Ray ray, inout float seed, Sphere[Objects] spheres) {
	vec3 incomingLight = vec3(0,0,0);
	vec3 rayColor = vec3(1,1,1);
	for (int i = 0; i <= MaxBounces; i++) {
		HitInfo hitInfo = CalculateCollision(ray, spheres);
		if(hitInfo.hit) {
			ray.origin = hitInfo.hitPoint;
			vec3 PerfectReflection = GetReflected(ray.dir, hitInfo);
			vec3 Diffuse = (DiffuseReflected(hitInfo.normal));
			ray.dir = lerp(PerfectReflection, Diffuse, hitInfo.sphere.roughness);
			//ray.dir = normalize(hitInfo.normal + PerfectReflection + Diffuse); //
			vec3 emittedLight = hitInfo.sphere.emissionColor * hitInfo.sphere.emissionStrength;
			if(clamp(seed, 0.0, 1.0) > 0.5) {
				incomingLight += emittedLight * rayColor;
				rayColor *= hitInfo.sphere.color;
			} else {
				incomingLight += emittedLight * rayColor;
				rayColor *= hitInfo.sphere.color;
			}

		} else {
			incomingLight += GetEnvironmentLight(ray) * rayColor;
			break;
		}
	}

	return incomingLight;
}


vec3 FarPlanePositionFromUV(vec2 uv) {
	float x_ndc = 2.0 * uv.x - 1.0;
	float y_ndc = 1.0 - 2.0 * uv.y;
	//vec3 P_view = vec3(x_ndc * tan(FOV/2.0) * AspectRatio, y_ndc * tan(FOV/2.0), -FarPlane);
	vec3 P_view = vec3(x_ndc * tan(FOV/2.0) * AspectRatio, y_ndc * tan(FOV/2.0), -FarPlane);
	return P_view;
}

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	COLOR = vec4(0,0,0,1);
	Sphere spheres[Objects];
	for (int i = 0; i < Objects; i++) {
		spheres[i].color = colors[i];
		spheres[i].position = points[i];
		spheres[i].radius = radii[i];
		spheres[i].roughness = roughness[i];
		spheres[i].emissionColor = emissionColor[i];
		spheres[i].emissionStrength = emisionStrength[i];
	}
	Ray r;
	for(int b = 0; b < RaysPerPixel; b++) {
		State = noise(UV);
		vec3 target = (InvViewMatrix * vec4(FarPlanePositionFromUV(UV), 1.0)).xyz;
		//State = RandomNormalDistribution(State);
		r.origin = (InvViewMatrix * vec4((2.0 * UV.x - 1.0) * AspectRatio, (1.0 - 2.0 * UV.y), -NearPlane, 1.0)).xyz;
		r.dir = normalize(target - r.origin);
		int bounces = 0;
		COLOR += vec4(Trace(r, State, spheres), 1.0);
	}
	float divisor = 1.0 / float(RaysPerPixel);
	vec4 prevColor = vec4(texture(SCREEN_TEXTURE, SCREEN_UV).rgb, 1.0);
	//COLOR = prevColor;
	//COLOR = vec4(COLOR.rgb * divisor, 1.0);
	//COLOR = vec4(UV, 0.0,1.0);
	COLOR = (vec4(COLOR.rgb * divisor, 1.0) * (1.0 - PreviousFrameWeight)) + PreviousFrameWeight * prevColor;
}