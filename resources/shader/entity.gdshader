shader_type canvas_item;

uniform vec4 vfx_color: source_color = vec4(1.0); 
uniform bool active                  = false;

void fragment() {
	if (active) {
		COLOR = vec4(vfx_color.rgb, texture(TEXTURE, UV).a);
	} else {
		COLOR = texture(TEXTURE, UV);
	}
}
