# Test — Shader & VFX experiments

Тестовая сцена для экспериментов с шейдерами и VFX Graph.

## Shaders

- **Glow.shadergraph** — свечение (post-process style)
- **Lighting.shadergraph** — освещение / тональное картирование
- **Pixielation.shadergraph** — пикселизация экрана
- **MonoFS.shadergraph** — монохромный вывод через `mono.hlsl`
- **mono.hlsl** — монохромный дизеринг с Bayer 4×4, 10 паттернов (0..9)

## VFX

- **New VFX.vfx** — партиклы-хвосты (спаунер → Initialize → Update → Output)
- **ligthing.vfx** — VFX молнии
- **ligthing Tests.vfx** — тестовый вариант молний
- **TestEffect.asset** — VFX-эффект-ассет

## Materials

- **glow.mat** — материал для Glow.shadergraph
- **pixelationFSH.mat** — материал для Pixielation.shadergraph

## Pipeline

- **New Universal Render Pipeline Asset.asset** — кастомный URP-ассет для тестов

## Зависимости

- `com.unity.visualeffectgraph` — добавлен в проект
