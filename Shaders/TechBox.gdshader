shader_type spatial;


uniform float xrot;
uniform float yrot;
uniform float zrot;

uniform sampler2D noise1;

//rot == (phi, theta, psi)
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

void vertex() {
	//VERTEX = VERTEX * ProcessRotation(vec3(TIME * xrot, TIME * yrot, TIME * zrot));
	//NORMAL = NORMAL * ProcessRotation(vec3(TIME * xrot, TIME * yrot, TIME * zrot));
	VERTEX = VERTEX * ProcessRotation(vec3(xrot, yrot, zrot));
	NORMAL = NORMAL * ProcessRotation(vec3(xrot, yrot, zrot));
	// Called for every vertex the material is visible on.
}

void fragment() {
	//ProcessRotation(vec3(TIME * xrot, TIME * yrot, TIME * zrot));
	//ALPHA = texture(noise1, vec2(UV.x + sin(TIME), UV.y + cos(TIME))).r;
	//ALBEDO = NORMAL * transpose(ProcessRotation(vec3(TIME * xrot,TIME *yrot,TIME * zrot)));
	// Called for every pixel the material is visible on.
}
