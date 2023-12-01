#version 440 core

out vec4 fragColor;

in vec2 uv;
in vec3 fragPos;
in vec3 normals;


uniform float aspectRatio;
uniform int health;

vec4 makeSquare(vec2 uvs, vec2 offset, float size)
{
	uvs += offset;
	vec4 square = 1-vec4(step(uvs, vec2(0.5 - size)) + (1 - step(uvs, vec2(0.5 + size))), 0, 1);
	if (square.x < 1 || square.y < 1) square = vec4(0,0,0,1);
	return square;
}

void main() {
	vec2 newuv = vec2(uv.x * aspectRatio, uv.y);
	const vec4 green = vec4(0,1,0,1);
    vec4 color = vec4(0);
    
	vec4 s1 = makeSquare(newuv, vec2(0.1,0.45), 0.03) * green;
	vec4 s2 = makeSquare(newuv, vec2(0,0.45), 0.03) * green;
	vec4 s3 = makeSquare(newuv, vec2(-0.1,0.45), 0.03) * green;
	
	if (health > 0) color = s1;
	if (health > 1) color += s2;
	if (health > 2) color += s3;
	
	if (color.rgb == vec3(0)) color.a = 0;
	else color.a = 1;

    fragColor = color;
}