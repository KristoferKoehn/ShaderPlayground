shader_type spatial;
render_mode unshaded;

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

struct Sphere {
	vec3 position;
	float radius;
	vec3 color;
	float roughness;
	vec3 emissionColor;
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
			//added sphere in initialize
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

float RandomNormalDistribution(float prev) {
	float rand = noise(vec2(prev, State));
	float theta = 2.0 * PI * rand;
	float rho =  sqrt(-2.0 * log(noise(vec2(State,noise(vec2(rand, theta))))));
	return rho * cos(theta);
}

vec3 RandomDirection(float rng) {
	float x = RandomNormalDistribution(State);
	float y = RandomNormalDistribution(x);
	float z = RandomNormalDistribution(y);
	return normalize(vec3(x,y,z));
}

vec3 GetReflected(vec3 incoming, Sphere[Objects] spheres, int k) {
	vec3 lenNorm = normalize(spheres[k].position - incoming);
	float prod = dot(incoming, lenNorm);
	vec3 outgoing = incoming - lenNorm * (2.0 * prod);
	return outgoing;
}

vec3 DiffuseReflected(vec3 incoming) {
	vec3 direction = RandomDirection(State);
	return direction * sign(dot(normalize(incoming), direction));
}

vec3 Trace(Ray ray, inout float seed, Sphere[Objects] spheres) {
	
	vec3 incomingLight = vec3(0,0,0);
	vec3 rayColor = vec3(1,1,1);
	for (int i = 0; i <= MaxBounces; i++) {
		seed = noise(ray.dir.xz);
		HitInfo hitInfo = CalculateCollision(ray, spheres);
		if(hitInfo.hit) {
			ray.origin = hitInfo.hitPoint;
			ray.dir = DiffuseReflected(hitInfo.normal);
			incomingLight += hitInfo.sphere.emissionColor * rayColor;
			rayColor *= hitInfo.sphere.color;
		} else {
			break;
		}
	}
	
	
	return incomingLight;
}

float sigmoid(float x) {
	return (-2.5f * x / 0.2f)/(1.0f + abs(-2.5f * x/ 0.2f)) *  0.7f + 0.5f;
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

vec2 latlong(vec3 sphere) {
	float lat = acos(sphere.y / 1.0); //theta
	float lon = atan(sphere.x / sphere.z); //phi
	return vec2(lat, lon);
}

void fragment() {
	ALBEDO = vec3(0,0,0);
	Sphere spheres[Objects];
	for (int i = 0; i < Objects; i++) {
		spheres[i].color = colors[i];
		spheres[i].position = points[i];
		spheres[i].radius = radii[i];
		spheres[i].roughness = roughness[i];
		spheres[i].emissionColor = emissionColor[i];
	}
	
	for(int b = 0; b < RaysPerPixel; b++) {
		//State = noise(vec2(TIME, UV.y)); crazy noise pattern;
		State = noise(vec2(23, 33));
		vec3 target = (InvViewMatrix * vec4(FarPlanePositionFromUV(UV), 1.0)).xyz;
		Ray r;
		r.origin = (InvViewMatrix * vec4((2.0 * UV.x - 1.0) * AspectRatio, (1.0 - 2.0 * UV.y), -NearPlane, 1.0)).xyz;
		r.dir = normalize(target - r.origin);
		int bounces = 0;
		ALBEDO = Trace(r, State,spheres);
		
		HitInfo h = CalculateCollision(r,spheres);
		if (h.hit) {
			//ALBEDO = (ViewMatrix * vec4(r.origin, 1.0)).xyz;
			float y = h.normal.y;
			float x = h.normal.x;
			float z = h.normal.z;
			//ALBEDO = vec3((x + 1.0) / 2.0, (y + 1.0) / 2.0, (y + 1.0) / 2.0);
			//ALBEDO = texture(testImage, vec2((x + 1.0) / 4.0, -sigmoid(y + 1.0))).xyz;
			//ALBEDO = (vec4(r.dir, 1.0)).xyz;
			//ALBEDO = h.sphere.color;
		}
		
	}
	
}