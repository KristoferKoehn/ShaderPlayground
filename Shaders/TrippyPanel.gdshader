shader_type spatial;
render_mode unshaded;

uniform int RaysPerPixel = 1;
uniform int MaxBounces = 2;
uniform int Z_Resolution = 200;
uniform vec3 CameraPosition = vec3(0,0,3);
uniform vec3 CameraRotation = vec3(0,0,0);
uniform float NearPlane = 0.1;
uniform float FarPlane = 8;
uniform float FOV = 2;
uniform float AspectRatio = 1;

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
};

const int Objects = 5;

uniform vec3[Objects] points;
uniform vec3[Objects] colors : source_color;
uniform float[Objects] radii;
uniform float[Objects] roughness;
uniform vec3[Objects] emissionColor : source_color;

varying float State;

HitInfo CheckCollision(Ray ray, Sphere sphere) {
	HitInfo hitinfo;
	
	float a = dot(ray.dir, ray.dir);
	//float b = 2 * dot()
	
	return hitinfo;
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

vec3 DiffuseReflected(vec3 incoming, Sphere[Objects] spheres, int k) {
	vec3 direction = RandomDirection(State);
	return direction * sign(dot(normalize(incoming), direction));
}

mat3 ProcessRotationZYX(vec3 rot) {
	float phi = rot.x;
	float theta = rot.y;
	float psi = rot.z;
	mat3 PositionalMatrix;

	PositionalMatrix[0][0] = cos(theta) * cos(psi) + sin(theta) * sin(phi) * sin(psi); //done?
	PositionalMatrix[1][0] = cos(phi) * sin(psi); //done?
	PositionalMatrix[2][0] = cos(theta) * sin(phi) * sin(psi) - cos(psi) * sin(theta); //done?
	PositionalMatrix[0][1] = -cos(theta) * sin(psi) + cos(psi) * sin(theta) * sin(phi); //done?
	PositionalMatrix[1][1] = cos(phi) * cos(psi); //done?
	PositionalMatrix[2][1] = cos(theta) * cos(psi) * sin(phi) + sin(theta) * sin(psi);
	PositionalMatrix[0][2] = cos(phi) * sin(theta);
	PositionalMatrix[1][2] = -sin(phi);
	PositionalMatrix[2][2] = cos(theta) * cos(phi);
	return PositionalMatrix;
}

mat4 ProjectionMatrix() {
	float z_n = NearPlane;
	float z_f = FarPlane;
	
	mat4 proj = mat4(0);
	vec4 column1 = vec4(1.0/(AspectRatio * tan(FOV/2.0)),0,0,0);
	vec4 column2 = vec4(0,1.0/(tan(FOV/2.0)),0,0);
	vec4 column3 = vec4(0,0,-((z_f + z_n)/(z_f - z_n)),-1);
	vec4 column4 = vec4(0,0,-((2.0 * z_f * z_n)/(z_f - z_n)),0);
	
	proj[0] = column1;
	proj[1] = column2;
	proj[2] = column3;
	proj[3] = column4;
	
	return proj;
}

mat4 InvProjectionMatrix() {
	return inverse(ProjectionMatrix());
}

vec3 FarPlanePositionFromUV(vec2 uv) {
	vec3 NearPlaneOrigin = vec3((uv.x - 1.0) * 4.0, uv.y, NearPlane);
	return vec3((NearPlaneOrigin.x -FOV/2.0) * FOV, (NearPlaneOrigin.y), NearPlane-FarPlane);
}

void vertex() {
	// Called for every vertex the material is visible on.
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
		State = noise(UV);
		vec3 target = FarPlanePositionFromUV(UV) * ProcessRotationZYX(CameraRotation) + CameraPosition;
		target = (normalize(target) * FarPlane)/ float(Z_Resolution);
		
		Ray r;
		r.origin = vec3(UV.x * 4.0, UV.y, 0.1) * ProcessRotationZYX(CameraRotation) + CameraPosition;
		r.dir = target;
		
		vec3 incomingLight = vec3(0,0,0);
		vec3 rayColor = vec3(1,1,1);
		vec3 seekPoint =  vec3(UV.x * 4.0, UV.y, 0.1) * ProcessRotationZYX(CameraRotation) + CameraPosition;
		int bounces = 0;
		for (int i = 0; i < Z_Resolution; i++) {
			seekPoint += target;
			for (int k = 0; k < Objects; k++) {
				State = noise(UV);
				if (length(seekPoint - spheres[k].position) <= spheres[k].radius) {
					incomingLight += rayColor * spheres[k].emissionColor;
					rayColor *= spheres[k].color;
					vec3 reflected = (GetReflected(target, spheres, k))/2.0;
					vec3 RoughnessVector = DiffuseReflected(seekPoint - spheres[k].position, spheres, k) * clamp(spheres[k].roughness, 0,1.0);
					target = (reflected + RoughnessVector)/2.0;
					seekPoint += target;
					bounces++;
				}
			}
			
			if (bounces >= MaxBounces) {
				break;
			}
			ALBEDO += incomingLight;
		}
	}
}