#version 440 core

out vec4 fragColor;

in vec3 Normals;
in vec3 FragPos;
in vec2 uv;

uniform int state;
uniform sampler2D explosionTex;
uniform float time;
uniform vec3 usercolor;
vec2 dim = vec2(5,5);

const float speed = 24;


void main()
{
	if (state == 0)
	{		
 		fragColor = vec4(usercolor,1);
	}
	else 
	{
		float totalFrames = dim.x * dim.y;
		if (time > 1/speed * totalFrames) return;

		float frame = floor(mod(time * speed + dim.x, totalFrames));

		vec2 newuv = uv;
		newuv.x = (uv.x + mod(frame, dim.x)) / dim.x;
		newuv.y = (uv.y - floor(frame / dim.x)) / dim.y;

		fragColor = texture(explosionTex,  newuv);
	}
}