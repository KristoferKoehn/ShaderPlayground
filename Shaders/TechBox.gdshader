shader_type spatial;
render_mode unshaded;

uniform mat4 test;
uniform float xrot;
uniform float yrot;
uniform float zrot;
//rot == (phi, theta, psi)
mat3 ProcessRotation(vec3 rot) {
	float phi = rot.x;
	float theta = rot.y;
	float psi = rot.z;
	mat3 PositionalMatrix;
	//one by one now
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

void vertex() {
	VERTEX = VERTEX * transpose(ProcessRotation(vec3(TIME * xrot,TIME *yrot,TIME * zrot)));
	// Called for every vertex the material is visible on.
}

void fragment() {
	if (UV.x > 0.666666 && UV.y < 0.5) {
		ALBEDO = vec3(0,0,1);
	}
	// Called for every pixel the material is visible on.
}
