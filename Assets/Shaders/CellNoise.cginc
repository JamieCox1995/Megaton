#include "Rand.cginc"

float2 cellNoise(float3 pt) {
	int xQuotient = int(floor(pt.x));
	int yQuotient = int(floor(pt.y));
	int zQuotient = int(floor(pt.z));

	float xFraction = pt.x - float(xQuotient);
	float yFraction = pt.y - float(yQuotient);
	float zFraction = pt.z - float(zQuotient);

	float dist1 = 999999.0;
	float dist2 = 999999.0;
	float3 cell;

	for (int z = -1; z <= 1; z++) {
		for (int y = -1; y <= 1; y++) {
			for (int x = -1; x <= 1; x++) {
				float randomValue = rand(float3(xQuotient + x, yQuotient + y, zQuotient + z));
				float xRandom = rand(randomValue);
				float yRandom = rand(xRandom);
				float zRandom = rand(yRandom);

				float3 cell = float3(xRandom, yRandom, zRandom);

				cell.x += (float(x) - xFraction);
				cell.y += (float(y) - yFraction);
				cell.z += (float(z) - zFraction);

				float dist = dot(cell, cell);

				if (dist < dist1) {
					dist2 = dist1;
					dist1 = dist;
				}
				else if (dist < dist2) {
					dist2 = dist;
				}
			}
		}
	}

	return float2(sqrt(dist1), sqrt(dist2));
}