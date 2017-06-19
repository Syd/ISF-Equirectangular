/*
{
  "CATEGORIES" : [
    "Equirectangular",
    "VR"
  ],
  "DESCRIPTION" : "ISF Equirectangular shader",
  "ISFVSN" : "2",
  "INPUTS" : [
    {
      "NAME" : "inputImage",
      "TYPE" : "image"
    },
    {
      "NAME" : "inpPitch",
      "TYPE" : "float",
      "DEFAULT" : 0.5
    },
    {
      "NAME" : "inpRoll",
      "TYPE" : "float",
      "DEFAULT" : 0.5
    },
    {
      "NAME" : "inpYaw",
      "TYPE" : "float",
      "MAX" : 1,
      "DEFAULT" : 0.5,
      "MIN" : 0
    },
    {
      "NAME" : "inpxShift",
      "TYPE" : "float",
      "DEFAULT" : 0.5
    },
    {
      "NAME" : "inpyShift",
      "TYPE" : "float",
      "DEFAULT" : 0.5
    }
  ],
  "CREDIT" : "Daniel Arnett, Scott Wisely"
}
*/

vec3 PRotateX(vec3 p, float theta)
{
   vec3 q;
   q.x = p.x;
   q.y = p.y * cos(theta) + p.z * sin(theta);
   q.z = -p.y * sin(theta) + p.z * cos(theta);
   return(q);
  return(q);
}

vec3 PRotateY(vec3 p, float theta)
{
   vec3 q;

   q.x = p.x * cos(theta) - p.z * sin(theta);
   q.y = p.y;
   q.z = p.x * sin(theta) + p.z * cos(theta);
   return(q);
}

vec3 PRotateZ(vec3 p, float theta)
{
   vec3 q;

   q.x = p.x * cos(theta) + p.y * sin(theta);
   q.y = -p.x * sin(theta) + p.y * cos(theta);
   q.z = p.z;
   return(q);
}

float PI = 3.141592653589;
vec4 inputColour = vec4(inpxShift,inpyShift,0.5,0.5);
vec4 iMouse = vec4(inpPitch,inpYaw,inpRoll,0.0);
vec2 iResolution = RENDERSIZE;
void mainImage(out vec4 fragColor, in vec2 fragCoord)
{
	vec2 shiftXYInput = (vec2(2.0,2.0) * vec2(inputColour.r, inputColour.g) - vec2(1.0,1.0)) * iResolution.xy;
	// Get inputs from Resolume
	float rotateXInput = iMouse.x / iResolution.x - 0.5;
	float rotateZInput = (iMouse.z / iResolution.x) - 0.5; // -0.5 to 0.5
	float rotateYInput = (iMouse.w / iResolution.y) - 0.5; // -0.5 to 0.5
	// Normalize the position of the destination pixel from -1.0 to 1.0
	vec2 pos = 2.0*(fragCoord.xy / iResolution.xy - 0.5);
	// Radius of the pixel from the center
	float r = sqrt(pos.x*pos.x + pos.y*pos.y);
	// Don't bother with pixels outside of the fisheye circle
	if (1.0 < r) {
		return;
	}
	float phi;
	float latitude = (1.0 - r)*(PI / 2.0);
	float longitude;
	float u;
	float v;
	// The ray into the scene
	vec3 p;
	// Output color. In our case the color of the source pixel
	vec3 col;
	// Set the source pixel's coordinates
	vec2 outCoord;
	// Calculate longitude
	if (r == 0.0) {
		longitude = 0.0;
	}
	else if (pos.x < 0.0) {
		longitude = PI - asin(pos.y / r);
	}
	else if (pos.x >= 0.0) {
		longitude = asin(pos.y / r);
	}
	// Perform z rotation.
	longitude += rotateZInput * 2.0 * PI;
	if (longitude < 0.0) {
		longitude += 2.0*PI;
	}
	// Turn the latitude and longitude into a 3D ray
	p.x = cos(latitude) * cos(longitude);
	p.y = cos(latitude) * sin(longitude);
	p.z = sin(latitude);
	// Rotate the value based on the user input
	p = PRotateX(p, 2.0 * PI * rotateXInput);
	p = PRotateY(p, 2.0 * PI * rotateYInput);
	// Get the source pixel latitude and longitude
	latitude = asin(p.z);
	longitude = -acos(p.x / cos(latitude));
	// Get the source pixel radius from center
	r = 1.0 - latitude/(PI / 2.0);
	// Disregard all images outside of the fisheye circle
	if (r > 1.0) {
		return;
	}

	// Manually implement `phi = atan2(p.y, p.x);`
	if (p.x > 0.0) {
		phi = atan(p.y / p.x);
	}
	else if (p.x < 0.0 && p.y >= 0.0) {
		phi = atan(p.y / p.x) + PI;
	}
	else if (p.x < 0.0 && p.y < 0.0) {
		phi = atan(p.y / p.x) - PI;
	}
	else if (p.x == 0.0 && p.y > 0.0) {
		phi = PI / 2.0;
	}
	else if (p.x == 0.0 && p.y < 0.0) {
		phi = -PI / 2.0;
	}
	if (phi < 0.0) {
		phi += 2.0*PI;
	}

	// Get the position of the output pixel
	u = iResolution.x * r * cos(phi);
	v = iResolution.y * r * sin(phi);
	outCoord.x = float(u) / 2.0 + iResolution.x / 2.0;
	outCoord.y = float(v) / 2.0 + iResolution.y / 2.0;

	outCoord += shiftXYInput;
	// Get the color of the source pixel
	//col = texture2D(inputImage, outCoord.xy / iResolution.xy).xyz;

	col = IMG_PIXEL(inputImage,outCoord.xy).xyz;
	// Set the color of the destination pixel to the color of the source pixel.
	fragColor = vec4(col, 1.0);
	// fragColor = vec4(0.0,255.0,0.0,1.0);
}

//thx Eric @ Magic Music Visuals
void main(void) {
    mainImage(gl_FragColor, gl_FragCoord.xy);
}
