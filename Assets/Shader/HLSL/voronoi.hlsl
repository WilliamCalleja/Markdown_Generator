float2 pmg_cell_size;
float pmg_noise_scale;

float4 inverse_lerp(const float4 a, const float4 b, const float4 t) { return (t - a)/(b - a); }
float3 inverse_lerp(const float3 a, const float3 b, const float3 t) { return (t - a)/(b - a); }
float2 inverse_lerp(const float2 a, const float2 b, const float2 t) { return (t - a)/(b - a); }
float inverse_lerp(const float a, const float b, const float t) { return (t - a)/(b - a); }

float2 grad_noise_dir(float2 p)
{
    p = p % 289;
    float x = (34 * p.x + 1) * p.x % 289 + p.y;
    x = (34 * x + 1) * x % 289;
    x = frac(x / 41) * 2 - 1;
    return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
}
float grad_noise(float2 p)
{
    const float2 ip = floor(p);
    float2 fp = frac(p);
    const float d00 = dot(grad_noise_dir(ip), fp);
    const float d01 = dot(grad_noise_dir(ip + float2(0, 1)), fp - float2(0, 1));
    const float d10 = dot(grad_noise_dir(ip + float2(1, 0)), fp - float2(1, 0));
    const float d11 = dot(grad_noise_dir(ip + float2(1, 1)), fp - float2(1, 1));
    fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
    return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
}
float gradient_noise(const float2 uv, const float scale)
{
    return grad_noise(uv * scale) + 0.5;
}
float2 get_cell_offset(const float2 cell)
{
    float2 origin = cell * pmg_cell_size;
    if(cell.y%2 == 0)
    {
        origin.x += (pmg_cell_size.x/2.0);
    }
    float offset_x = gradient_noise(origin, pmg_noise_scale) * pmg_cell_size.x;
    float offset_y = gradient_noise(origin * 2, pmg_noise_scale) * pmg_cell_size.y;
    return origin + float2(offset_x, offset_y);
}
float get_cell_distance(float3 position_ws_a, const float2 cell)
{
    return distance(position_ws_a.xy, get_cell_offset(cell));
}
float get_closest_distance(const float3 position_ws_a, const float2 cell)
{
    const float max_distance = distance(0, pmg_cell_size);
    float dist = inverse_lerp(0, max_distance, get_cell_distance(position_ws_a, cell));

    dist = min(dist, inverse_lerp(0, max_distance, get_cell_distance(position_ws_a, cell + float2(0,1))));
    dist = min(dist, inverse_lerp(0, max_distance, get_cell_distance(position_ws_a, cell + float2(0,-1))));
    dist = min(dist, inverse_lerp(0, max_distance, get_cell_distance(position_ws_a, cell + float2(-1,0))));
    dist = min(dist, inverse_lerp(0, max_distance, get_cell_distance(position_ws_a, cell + float2(1,0))));
    dist = min(dist, inverse_lerp(0, max_distance, get_cell_distance(position_ws_a, cell + float2(1,1))));
    dist = min(dist, inverse_lerp(0, max_distance, get_cell_distance(position_ws_a, cell + float2(1,-1))));
    dist = min(dist, inverse_lerp(0, max_distance, get_cell_distance(position_ws_a, cell + float2(-1,-1))));
    dist = min(dist, inverse_lerp(0, max_distance, get_cell_distance(position_ws_a, cell + float2(-1,1))));
    return dist;
}
void voronoi_float(float3 position_ws_a, out float mask)
{
    const float2 cell = float2(floor(position_ws_a.x/pmg_cell_size.x), floor(position_ws_a.y/pmg_cell_size.y));
    const float dist = get_closest_distance(position_ws_a, cell);

    mask = dist;
}