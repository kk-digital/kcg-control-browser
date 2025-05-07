namespace user_profile_management;

public class RealisticViewportGenerator
{
    private static readonly Random random = new Random();

    // Weighted popular desktop screen resolutions (width, height, weight)
    private static readonly (int width, int height, double weight)[] screenResolutions = new (int, int, double)[]
    {
        (1920, 1080, 0.40),
        (1366, 768, 0.25),
        (1536, 864, 0.10),
        (1440, 900, 0.07),
        (1600, 900, 0.05),
        (1280, 720, 0.04),
        (1280, 800, 0.03),
        (1680, 1050, 0.03),
        (2560, 1440, 0.02),
        (1360, 768, 0.01)
    };
    
   // Returns realistic screen, browser window, and viewport sizes.
    public static (int screenWidth, int screenHeight, int windowWidth, int windowHeight, int viewportWidth, int viewportHeight) GetRandomWindowAndViewport()
    {
        // 1. Weighted random selection of screen resolution
        double r = random.NextDouble();
        double cumulative = 0.0;
        int selected = 0;
        for (int i = 0; i < screenResolutions.Length; i++)
        {
            cumulative += screenResolutions[i].weight;
            if (r <= cumulative)
            {
                selected = i;
                break;
            }
        }
        int screenWidth = screenResolutions[selected].width;
        int screenHeight = screenResolutions[selected].height;

        // 2. Simulate browser window size
        // Subtract typical OS taskbar height (e.g., 40-50px) and window chrome (e.g., 30-50px)
        int taskbarHeight = random.Next(40, 51); // Windows taskbar approx height
        int windowChromeHeight = random.Next(30, 51); // Browser UI chrome (tabs, address bar, bookmarks)
        int totalVerticalChrome = taskbarHeight + windowChromeHeight;

        // For width, subtract window borders and possible sidebars (random 0-80px)
        int windowWidthChrome = random.Next(0, 81);

        int windowWidth = screenWidth - windowWidthChrome;
        int windowHeight = screenHeight - totalVerticalChrome;

        // 3. Simulate viewport size by subtracting scrollbar sizes
        // Scrollbar sizes typically 15-17px on desktop browsers
        int verticalScrollbarWidth = random.Next(15, 18);
        int horizontalScrollbarHeight = random.Next(15, 18);

        int viewportWidth = windowWidth - verticalScrollbarWidth;
        int viewportHeight = windowHeight - horizontalScrollbarHeight;

        // Clamp minimum viewport sizes to avoid unrealistic tiny viewports
        if (viewportWidth < 300) viewportWidth = 300;
        if (viewportHeight < 200) viewportHeight = 200;

        return (screenWidth, screenHeight, windowWidth, windowHeight, viewportWidth, viewportHeight);
    }
}