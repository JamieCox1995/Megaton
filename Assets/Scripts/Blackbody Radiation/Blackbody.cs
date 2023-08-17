using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Blackbody
{
    //[Range(0, 20000)]
    //public double temperature = 300;

    //[SerializeField]
    //private Material materialToChange;

    // Planck constant, J*s
    private const double h = 6.626070040e-34;

    // Speed of light, m/s
    private const double c = 299792458;

    // Boltzmann constant, J/K
    private const double k = 1.38064852e-23;

    //private const double intensityScale = 1e-8;

    // Draper point, should begin emitting visible light
    private static readonly Vector2 intensityMultiplierA = new Vector2(798f, 0.05f);

    // Illuminant D65, "white"
    private static readonly Vector2 intensityMultiplierB = new Vector2(6504f, 5f);

    // Maximum value
    private static readonly Vector2 intensityMultiplierC = new Vector2(50000f, 50f);

    private const float magnitudeThreshold = 0.0000000000001f;
	
	// Update is called once per frame
	/*void Update()
    {
        Color colour = GetRgbColour(temperature);

        Renderer renderer = GetComponent<Renderer>();

        if (renderer != null)
        {

            Material[] mats = renderer.materials;

            for(int i = 0; i < mats.Length; i++)
            {
                if (renderer.materials[i].name == materialToChange.name + " (Instance)")
                {
                    renderer.materials[i].SetColor("_EmissionColor", colour);
                }
            }

            //renderer.material.SetColor("_EmissionColor", colour);
        }
    }*/

    /*
        CIE to sRGB from https://en.wikipedia.org/wiki/SRGB
    */
    public static Color GetRgbColour(double temperature)
    {
        Vector3 cieColour = GetCieColour(temperature);

        float r = 3.2406f * cieColour.x - 1.5372f * cieColour.y - 0.4986f * cieColour.z;
        float g = -0.9689f * cieColour.x + 1.8758f * cieColour.y + 0.0415f * cieColour.z;
        float b = 0.0557f * cieColour.x - 0.2040f * cieColour.y + 1.0570f * cieColour.z;

        float magnitude = Mathf.Max(r, g, b);

        if (magnitude < magnitudeThreshold) return Color.black;

        float multiplier = (float)TemperatureIntensityMultiplier(temperature) / magnitude;

        return new Color(r, g, b) * multiplier;
    }

    /*
        CIE colour approximations from http://jcgt.org/published/0002/02/01/paper.pdf
    */
    private static Vector3 GetCieColour(double temperature)
    {
        double x = 0;
        double y = 0;
        double z = 0;

        int dW = 1;
        double deltaWavelength = dW / 1e09;

        for (int w = 390; w <= 700; w += dW)
        {
            double wavelength = w / 1e09;
            double frequency = (float)(c / wavelength);

            double dx = Cie64ColourMatchX(w) * GetIntensity(wavelength, temperature) * deltaWavelength;
            double dy = Cie64ColourMatchY(w) * GetIntensity(wavelength, temperature) * deltaWavelength;
            double dz = Cie64ColourMatchZ(w) * GetIntensity(wavelength, temperature) * deltaWavelength;

            x += dx;
            y += dy;
            z += dz;
        }

        Vector3 cie = new Vector3((float)x, (float)y, (float)z);

        return cie;
    }

    private static double Cie64ColourMatchX(double wavelength)
    {
        double alpha1 = 0.398;
        double beta1 = -1250.0;
        double gamma1 = Math.Log((wavelength + 570.1) / 1014.0);

        double alpha2 = 1.132;
        double beta2 = -234;
        double gamma2 = Math.Log((1338.0 - wavelength) / 743.5);

        return CieMatch(alpha1, beta1, gamma1) + CieMatch(alpha2, beta2, gamma2);
    }

    private static double Cie64ColourMatchY(double wavelength)
    {
        double alpha = 1.011;
        double beta = -0.5;
        double gamma = (wavelength - 556.1) / 46.14;

        return CieMatch(alpha, beta, gamma);
    }

    private static double Cie64ColourMatchZ(double wavelength)
    {
        double alpha = 2.060;
        double beta = -32.0;
        double gamma = Math.Log((wavelength - 265.8) / 180.4);

        return CieMatch(alpha, beta, gamma);
    }

    private static double Cie31ColourMatchX(double wavelength)
    {
        double alpha1 = 1.065;
        double beta1 = -0.5;
        double gamma1 = (wavelength - 595.8) / 33.33;

        double alpha2 = 0.366;
        double beta2 = -0.5;
        double gamma2 = (wavelength - 446.8) / 19.44;

        return CieMatch(alpha1, beta1, gamma1) + CieMatch(alpha2, beta2, gamma2);
    }

    private static double Cie31ColourMatchY(double wavelength)
    {
        double alpha = 1.014;
        double beta = -0.5;
        double gamma = (Math.Log(wavelength) - Math.Log(556.3)) / 0.075;

        return CieMatch(alpha, beta, gamma);
    }

    private static double Cie31ColourMatchZ(double wavelength)
    {
        double alpha = 1.839;
        double beta = -0.5;
        double gamma = (Math.Log(wavelength) - Math.Log(449.8)) / 0.051;

        return CieMatch(alpha, beta, gamma);
    }

    private static double CieMatch(double alpha, double beta, double gamma)
    {
        return alpha * Math.Exp(beta * gamma * gamma);
    }

    private static double GetIntensity(double wavelength, double temperature)
    {
        return (8.0 * Math.PI * h * c) / (Math.Pow(wavelength, 5.0) * (Math.Exp((h * c) / (wavelength * k * temperature)) - 1.0));
    }

    private static double TemperatureIntensityMultiplier(double temperature)
    {
        double x1 = intensityMultiplierA.x;
        double x2 = intensityMultiplierB.x;
        double x3 = intensityMultiplierC.x;

        double y1 = intensityMultiplierA.y;
        double y2 = intensityMultiplierB.y;
        double y3 = intensityMultiplierC.y;

        double t1 = (y1 * (temperature - x2) * (temperature - x3)) / ((x1 - x2) * (x1 - x3));
        double t2 = (y2 * (temperature - x1) * (temperature - x3)) / ((x2 - x1) * (x2 - x3));
        double t3 = (y3 * (temperature - x1) * (temperature - x2)) / ((x3 - x1) * (x3 - x2));

        return t1 + t2 + t3;
    }
}
