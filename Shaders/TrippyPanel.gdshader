shader_type spatial;

uniform vec3 CameraPosition = vec3(0,0,3);
uniform vec3 CameraRotation = vec3(0,0,0);
uniform int Z_Resolution = 200;

uniform vec3[3] points;
//camera is at position (0,0,3)
//transform UV into perpendicular plane twice the size at (0,0,-3)
//0,0 top left, and goes to (-1, -1, -3)

mat3 ProcessRotation(vec3 rot) {
	float phi = rot.x;
	float theta = rot.y;
	float psi = rot.z;
	mat3 PositionalMatrix;

	PositionalMatrix[0][0] = cos(theta)*cos(psi);
	PositionalMatrix[1][0] = cos(phi)*sin(psi) + sin(phi) * sin(theta) * cos(psi);
	PositionalMatrix[2][0] = sin(phi) * sin(psi) - cos(phi)*sin(theta)*cos(psi);
	PositionalMatrix[0][1] = -cos(theta)*sin(psi);
	PositionalMatrix[1][1] = cos(phi)*cos(psi) - sin(phi) * sin(theta) * sin(psi);
	PositionalMatrix[2][1] = sin(phi) * cos(psi) + cos(phi)*sin(theta)*sin(psi);
	PositionalMatrix[0][2] = sin(theta);
	PositionalMatrix[1][2] = -sin(phi)*cos(theta);
	PositionalMatrix[2][2] = cos(phi)*cos(theta);
	return PositionalMatrix;
}

vec3 FarPlanePositionFromUV(vec2 uv) {
	return vec3((uv.x * 2.0) -1.0, (uv.y * 2.0) - 1.0, CameraPosition.z-6.0);
}

void vertex() {
	
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec3 TriangleNormal = cross(points[0] - points[1], points[0] - points[2]);
	
	vec3 target = FarPlanePositionFromUV(UV);
	target = (normalize(target) * 6.0)/ float(Z_Resolution);
	
	target = target * ProcessRotation(CameraRotation);
	
	vec3 seekPoint = CameraPosition;
	for (int i = 0; i < Z_Resolution; i++) {
		float N = dot(TriangleNormal,(CameraPosition + seekPoint - points[0]));
		if (N == 0.0) {
			
		}
	}
	// Called for every pixel the material is visible on.
}