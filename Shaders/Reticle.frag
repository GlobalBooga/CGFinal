#version 440 core

out vec4 fragColor;

in vec2 uv;
in vec3 FragPos;
in vec3 Normals;

uniform float aspectRatio;

float reticleSize = 0.06;
int reticleStyle = 4;
vec4 reticleColor = vec4(1,0,0,1);

void main() {
	float thickness = 0.0025;
	float style3Size = reticleSize - 0.03;
	if (reticleStyle == 1) thickness = 0.005;
	
	vec2 tempuv = vec2(uv.x * aspectRatio, uv.y);
	vec2 midpoint = vec2(0.64, 0.5);
	
	float dist = distance(tempuv,midpoint);
	vec4 circle = vec4(step(dist, reticleStyle == 3? style3Size : reticleSize));
	vec4 smallerCircle = vec4(step(dist, (reticleStyle == 3? style3Size : reticleSize)-thickness));
	
	vec4 reticle = (circle - smallerCircle) * reticleColor;
	
	if (reticleStyle == 1) 
	{
		float gap = 0.02f;
		if (abs(tempuv.x - midpoint.x) < gap || abs(tempuv.y - midpoint.y) < gap)
		{
			reticle -= vec4(1);
		}
	}
	else if (reticleStyle == 2) 
	{
		float extra = 0.03f;
		float tempThickness = thickness -0.0007f;
		if ((abs(tempuv.x - midpoint.x) < tempThickness && abs(tempuv.y - midpoint.y) < reticleSize + extra)|| (abs(tempuv.y - midpoint.y) < tempThickness && abs(tempuv.x - midpoint.x) < reticleSize+extra))
		{
			reticle += reticleColor;
		}
	}
	else if (reticleStyle == 3) 
	{
		float extra = 0.08f;
		float tempThickness = thickness -0.0007f;
		if ((abs(tempuv.x - midpoint.x) < tempThickness && abs(tempuv.y - midpoint.y) < style3Size + extra && abs(tempuv.y - midpoint.y) > style3Size) || 
		(abs(tempuv.y - midpoint.y) < tempThickness && abs(tempuv.x - midpoint.x) < style3Size+extra && abs(tempuv.x - midpoint.x) > style3Size))
		{
			reticle += reticleColor;
		}
	}
	else if (reticleStyle == 4)
	{
		float extra = 0.08f;
		float tempThickness = thickness -0.0007f;
		if ((abs(tempuv.x - midpoint.x) < tempThickness && abs(tempuv.y - midpoint.y) < style3Size + extra && abs(tempuv.y - midpoint.y) > style3Size) || 
		(abs(tempuv.y - midpoint.y) < tempThickness && abs(tempuv.x - midpoint.x) < style3Size+extra && abs(tempuv.x - midpoint.x) > style3Size))
		{
			reticle += reticleColor;
		}
	}
	else if (reticleStyle == 5)
	{
		reticle = vec4(0);
		float extra = 0.04f;
		if ((abs(tempuv.x - midpoint.x) < thickness && abs(tempuv.y - midpoint.y) < style3Size + extra && abs(tempuv.y - midpoint.y) > style3Size) || 
		(abs(tempuv.y - midpoint.y) < thickness && abs(tempuv.x - midpoint.x) < style3Size+extra && abs(tempuv.x - midpoint.x) > style3Size))
		{
			reticle += reticleColor;
		}
	}
	 
	fragColor = reticle;
}