#version 440 core

layout (location = 0) in vec3 vertPosition;
layout (location = 1) in vec3 aNormals;
layout (location = 2) in vec2 UV0;

out vec2 uv;
out vec3 FragPos;
out vec3 Normals;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
	vec4 pos =  vec4(vertPosition + vec3(0.225, 0, 0),1);
	 
	gl_Position = pos;
    uv = UV0;
	Normals = aNormals * mat3(transpose(inverse(model)));

	FragPos = vec3(pos); 
}