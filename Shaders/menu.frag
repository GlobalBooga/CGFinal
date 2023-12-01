#version 440 core

out vec4 fragColor;

in vec2 uv;
in vec3 FragPos;
in vec3 Normals;

uniform float aspectRatio;
uniform sampler2D menu1;
uniform sampler2D menu2;
uniform sampler2D menu3;
uniform sampler2D menu4;
uniform int menu;

void main() 
{
	vec2 tempuv = vec2(uv.x * aspectRatio, uv.y);

	if (menu == 0) fragColor = texture(menu1, tempuv);
	else if (menu == 1) fragColor = texture(menu2, tempuv);
	else if (menu == 2) fragColor = texture(menu3, tempuv);
	else if (menu == 3) fragColor = texture(menu4, tempuv);
}