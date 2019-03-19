Shader "Hidden/Off-Screen Particles Composite"{
	Properties{
	_MainTex("Base(RGB)", RECT) = "white"{}
}
SubShader{
	Pass{
	ZTest Always Cull Off ZWrite Off Fog{ Mode Off }
	Blend One SrcAlpha
	SetTexture[_MainTex]{ combine texture }
}
}
Fallback Off
}