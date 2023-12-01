#version 440 core

out vec4 fragColor;

in vec2 uv;
in vec3 fragPos;
in vec3 normals;

void main()
{
	vec4 color = vec4(1);
	
	fragColor = color;
}