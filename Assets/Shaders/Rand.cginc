float rand(float3 pt) {
	return frac(sin(dot(pt.xyz, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
}