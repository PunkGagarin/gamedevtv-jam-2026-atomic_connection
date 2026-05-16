// DitherMonoBayer4x4_Screen.hlsl
// Монохромный дизеринг с Bayer 4x4, адаптация под экран и выбор паттерна (0..9),
// единый скейл Scale (в пикселях).

// _ScreenParams: x = width, y = height. [web:128]

float GetThresholdScreen(float2 UV, float Scale, int pattern)
{
    float2 screenSize = _ScreenParams.xy;   // размер экрана в пикселях [web:123][web:128]

    // размер одной точки в пикселях (Scale), конвертим в кол-во блоков по экрану
    float2 cellSize = float2(Scale, Scale);
    float2 blocks = screenSize / cellSize;

    // локальные координаты внутри Bayer 4x4
    float2 cellUV = frac(UV * blocks) * 4.0;
    int2 p = (int2)cellUV; // 0..3

    const float bayer4x4[4][4] = {
        {  0.0f/16.0f,  8.0f/16.0f,  2.0f/16.0f, 10.0f/16.0f },
        { 12.0f/16.0f,  4.0f/16.0f, 14.0f/16.0f,  6.0f/16.0f },
        {  3.0f/16.0f, 11.0f/16.0f,  1.0f/16.0f,  9.0f/16.0f },
        { 15.0f/16.0f,  7.0f/16.0f, 13.0f/16.0f,  5.0f/16.0f }
    };

    float t = bayer4x4[p.y][p.x];

    // pattern 0: обычный Bayer 4x4
    if (pattern == 0)
        return t;

    // pattern 1: инвертированный Bayer
    if (pattern == 1)
        return 1.0 - t;

    // pattern 2: горизонтальное зеркало
    if (pattern == 2)
        return bayer4x4[p.y][3 - p.x];

    // pattern 3: вертикальное зеркало
    if (pattern == 3)
        return bayer4x4[3 - p.y][p.x];

    // pattern 4: транспонированный (повёрнутый)
    if (pattern == 4)
        return bayer4x4[p.x][p.y];

    // pattern 5: диагональный сдвиг по UV
    if (pattern == 5)
    {
        float2 cellUV2 = frac((UV + float2(0.25, 0.25)) * blocks) * 4.0;
        int2 p2 = (int2)cellUV2;
        return bayer4x4[p2.y][p2.x];
    }

    // pattern 6: «крупный» уровень (усредняем блок 2x2)
    if (pattern == 6)
    {
        int2 q = p / 2; // 0..1
        float tmp = 0.0;
        tmp += bayer4x4[q.y*2+0][q.x*2+0];
        tmp += bayer4x4[q.y*2+0][q.x*2+1];
        tmp += bayer4x4[q.y*2+1][q.x*2+0];
        tmp += bayer4x4[q.y*2+1][q.x*2+1];
        return tmp * 0.25;
    }

    // pattern 7: подчёркиваем только чётные клетки
    if (pattern == 7)
    {
        float v = bayer4x4[p.y][p.x];
        return ((p.x + p.y) & 1) == 0 ? v : v * 0.25;
    }

    // pattern 8: добавляем лёгкий синусный шум
    if (pattern == 8)
    {
        float noise = 0.25 * (sin(UV.x * 100.0) * sin(UV.y * 100.0));
        return saturate(t + noise);
    }

    // pattern 9: ступенчатые уровни (4 уровня)
    if (pattern == 9)
    {
        float v = bayer4x4[p.y][p.x];
        if (v < 0.25) return 0.125;
        if (v < 0.5)  return 0.375;
        if (v < 0.75) return 0.625;
        return 0.875;
    }

    // дефолт
    return t;
}

// Главная функция для Custom Function Node (File)
void DitherMonoBayer4x4_float(float Gray, float2 UV, float Scale, float PatternIndex, out float Out)
{
    int pattern = (int)round(saturate(PatternIndex) * 9.0); // 0..9
    float threshold = GetThresholdScreen(UV, Scale, pattern);
    Out = Gray > threshold ? 1.0 : 0.0;
}