using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarColors 
{
    public static List<Color> humanSkinTones = new List<Color>
    {
        // Tons mais claros
        new Color(1f, 1f, 1f, 1f),
        new Color(0.9843f, 0.9608f, 0.9373f, 1f),
        new Color(0.9686f, 0.9255f, 0.8863f, 1f),
        new Color(0.9569f, 0.9020f, 0.8471f, 1f),
        new Color(0.9412f, 0.8784f, 0.8118f, 1f),
        new Color(0.9294f, 0.8588f, 0.7804f, 1f),
        new Color(0.9137f, 0.8353f, 0.7490f, 1f),
        new Color(0.9020f, 0.8157f, 0.7216f, 1f),
        new Color(0.8863f, 0.7961f, 0.6941f, 1f),
        new Color(0.8745f, 0.7765f, 0.6667f, 1f),

        // Meio claro
        new Color(0.8627f, 0.7569f, 0.6431f, 1f),
        new Color(0.8471f, 0.7412f, 0.6196f, 1f),
        new Color(0.8392f, 0.7255f, 0.6000f, 1f),
        new Color(0.8275f, 0.7098f, 0.5804f, 1f),
        new Color(0.8157f, 0.6941f, 0.5608f, 1f),
        new Color(0.8039f, 0.6784f, 0.5451f, 1f),
        new Color(0.7922f, 0.6627f, 0.5255f, 1f),
        new Color(0.7804f, 0.6471f, 0.5098f, 1f),
        new Color(0.7686f, 0.6314f, 0.4941f, 1f),
        new Color(0.7569f, 0.6157f, 0.4784f, 1f),

        // Meio
        new Color(0.7451f, 0.6000f, 0.4627f, 1f),
        new Color(0.7333f, 0.5843f, 0.4510f, 1f),
        new Color(0.7216f, 0.5725f, 0.4353f, 1f),
        new Color(0.7098f, 0.5569f, 0.4235f, 1f),
        new Color(0.6980f, 0.5451f, 0.4118f, 1f),
        new Color(0.6863f, 0.5333f, 0.4000f, 1f),
        new Color(0.6745f, 0.5216f, 0.3882f, 1f),
        new Color(0.6627f, 0.5098f, 0.3765f, 1f),
        new Color(0.6510f, 0.4980f, 0.3647f, 1f),
        new Color(0.6392f, 0.4863f, 0.3529f, 1f),

        // Meio escuro
        new Color(0.6275f, 0.4745f, 0.3451f, 1f),
        new Color(0.6157f, 0.4627f, 0.3373f, 1f),
        new Color(0.6039f, 0.4549f, 0.3294f, 1f),
        new Color(0.5922f, 0.4431f, 0.3216f, 1f),
        new Color(0.5804f, 0.4353f, 0.3137f, 1f),
        new Color(0.5686f, 0.4235f, 0.3059f, 1f),
        new Color(0.5569f, 0.4157f, 0.2980f, 1f),
        new Color(0.5451f, 0.4078f, 0.2902f, 1f),
        new Color(0.5333f, 0.3961f, 0.2824f, 1f),
        new Color(0.5216f, 0.3882f, 0.2745f, 1f),

        // Tons mais escuros
        new Color(0.5137f, 0.3804f, 0.2706f, 1f),
        new Color(0.5059f, 0.3725f, 0.2667f, 1f),
        new Color(0.4980f, 0.3647f, 0.2627f, 1f),
        new Color(0.4902f, 0.3569f, 0.2588f, 1f),
        new Color(0.4824f, 0.3490f, 0.2549f, 1f),
        new Color(0.4745f, 0.3412f, 0.2510f, 1f),
        new Color(0.4667f, 0.3333f, 0.2471f, 1f),
        new Color(0.4588f, 0.3255f, 0.2431f, 1f),
        new Color(0.4510f, 0.3176f, 0.2392f, 1f),
        new Color(0.4157f, 0.3686f, 0.3686f, 1f)
    };

    public static List<Color> elfSkinTones = new List<Color>
    {
        new Color(0.9804f, 0.9882f, 1f, 1f),
        new Color(0.9686f, 0.9804f, 0.9922f, 1f),
        new Color(0.9608f, 0.9725f, 0.9882f, 1f),
        new Color(0.9529f, 0.9608f, 0.9765f, 1f),
        new Color(0.9451f, 0.9529f, 0.9686f, 1f),
        new Color(0.9373f, 0.9451f, 0.9608f, 1f),
        new Color(0.9294f, 0.9373f, 0.9529f, 1f),
        new Color(0.9216f, 0.9294f, 0.9451f, 1f),
        new Color(0.9137f, 0.9216f, 0.9373f, 1f),
        new Color(0.9059f, 0.9137f, 0.9294f, 1f),
        new Color(0.8980f, 0.9059f, 0.9216f, 1f),
        new Color(0.8902f, 0.8980f, 0.9137f, 1f),
        new Color(0.8824f, 0.8902f, 0.9059f, 1f),
        new Color(0.8745f, 0.8824f, 0.8980f, 1f),
        new Color(0.8667f, 0.8745f, 0.8902f, 1f),
        new Color(0.8588f, 0.8667f, 0.8824f, 1f),
        new Color(0.8510f, 0.8588f, 0.8745f, 1f),
        new Color(0.8431f, 0.8510f, 0.8667f, 1f),
        new Color(0.8353f, 0.8431f, 0.8588f, 1f),
        new Color(0.8275f, 0.8353f, 0.8510f, 1f),
        new Color(0.8196f, 0.8275f, 0.8431f, 1f),
        new Color(0.8118f, 0.8196f, 0.8353f, 1f),
        new Color(0.8039f, 0.8118f, 0.8275f, 1f),
        new Color(0.7961f, 0.8039f, 0.8196f, 1f),
        new Color(0.7882f, 0.7961f, 0.8118f, 1f),
        new Color(0.7804f, 0.7882f, 0.8039f, 1f),
        new Color(0.7725f, 0.7804f, 0.7961f, 1f),
        new Color(0.7647f, 0.7725f, 0.7882f, 1f),
        new Color(0.7569f, 0.7647f, 0.7804f, 1f),
        new Color(0.7490f, 0.7569f, 0.7725f, 1f)
    };
}
