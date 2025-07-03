shader_type spatial;
render_mode unshaded;

uniform vec4 color: source_color;
uniform bool checkerEnabled = true;

void vertex() {
}

void fragment() {
	float size = 0.1;
	float checker = float(int(UV.x/size) + int(UV.y/size) % 2 == 0);
	checker *= float(checkerEnabled);
	ALBEDO = color.rgb * (1.0 + checker);
}
