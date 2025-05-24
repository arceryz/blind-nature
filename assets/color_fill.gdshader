shader_type spatial;
render_mode unshaded;

uniform vec4 color: source_color;

void vertex() {
}

void fragment() {
	float size = 0.1;
	float checker = float(int(UV.x/size) + int(UV.y/size) % 2 == 0);
	ALBEDO = color.rgb * (1.0 + checker);
}
