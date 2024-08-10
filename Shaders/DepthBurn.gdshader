shader_type spatial;


uniform vec3 PlaneNormal;
uniform float PlaneOffset;

uniform sampler2D noise;

float PointDistance(vec3 pos) {
	vec3 NormPlane = normalize(PlaneNormal);
	
	//float timeOffset = sin(TIME) * 2.0 + 0.6;
	
	float S = NormPlane.x * pos.x + NormPlane.y * pos.y + NormPlane.z * pos.z + PlaneOffset;
	//float S = NormPlane.x * pos.x + NormPlane.y * pos.y + NormPlane.z * pos.z + timeOffset;
	return S;
}

void fragment() {
	vec3 world_vertex = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
	
	float S = PointDistance(world_vertex);
	float nOffset = texture(noise, vec2 (world_vertex.x - TIME/4.0, world_vertex.y- TIME/3.0 )).r / 4.0;
	//float nOffset = texture(noise, UV).r / 4.0;
	if (S > nOffset - 0.2 && S < nOffset + 0.2) {
		ALBEDO = vec3(0, clamp(nOffset - S,0.0, 10.0)*100.0, clamp(nOffset - S,0.0, 10.0)*30.0 );
	} else if (S > nOffset + 0.2) {
		ALPHA = 0.0;
	}
	
	//bright green before black, and it has to be actual 0,0,0 black. 
	
}
