#version 440 core

out vec4 fragColor;

in vec2 uv;
in vec3 fragPos;
in vec3 normals;

struct PointLight{
	vec3 lightColor;
	vec3 lightPos;
	float lightIntensity;
};

uniform PointLight pointLights[100];
uniform int numPointLights;
uniform vec3 viewPos;

vec3 HandleLighting();

void main()
{
	vec4 color = vec4(1);
	
	fragColor = color * vec4(HandleLighting(), 1);
}


vec3 HandleLighting()
{
	vec3 outCol;

	for(int i = 0; i < numPointLights; i++)
	{
		const PointLight light = pointLights[i];

		// lighting (phong shading)
		const float ambientStrength = 0.1;
		vec3 ambient = ambientStrength * light.lightColor;

		vec3 norms = normalize(normals);
		vec3 lightDir = normalize(light.lightPos - fragPos);

		float diff = max(dot(norms, lightDir), 0);
		vec3 diffuse = diff * light.lightColor;

		const float shininess = 128;
		float specularStrength = 1;
		vec3 viewDir = normalize(viewPos - fragPos);
		vec3 reflectionDir = reflect(-lightDir, norms);
		float spec = pow(max(dot(viewDir, reflectionDir), 0), shininess);
		vec3 specular = specularStrength * spec * light.lightColor;

		outCol += (ambient + diffuse + specular) * light.lightIntensity;
	}

	return outCol;
}