Shader "Solid Color" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
	}
	SubShader {
		Cull Off
		Pass { Color [_Color] }
	}
}