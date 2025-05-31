shader_type spatial;
render_mode unshaded;

varying flat vec3 color;

void vertex() {
	color = COLOR.rgb;
}

void fragment() {
	ALBEDO.rgb = color;
}