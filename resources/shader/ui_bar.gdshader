shader_type canvas_item;

uniform vec4  color: source_color = vec4(1.0);
uniform float ratio               = 1.0;
uniform vec2  size                = vec2(128.0, 32.0);
uniform float outline             = 10.0;

void fragment() {
	if (UV.x <= ratio) {
		COLOR = (color + vec4(vec3(0.0, 1.0, 0.0) * UV.x, 1.0)) * vec4(vec3(1.0) * (1.0 - UV.y), 1.0);
	} else {
		COLOR = vec4(vec3(0.0) * UV.y, 1.0);
	}

	vec2 border = vec2(
		outline / size.x,
		outline / size.y
	);
	if (UV.x < border.x || UV.x > 1.0 - border.x) {
		COLOR = vec4(1.0);
	}
	if (UV.y < border.y || UV.y > 1.0 - border.y) {
		COLOR = vec4(1.0);
	}
}
