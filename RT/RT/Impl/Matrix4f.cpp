#include "../Headers/Matrix4f.h"


namespace TEMP_MATS
{
    //The determinant/inverse of a 4x4 is found by recursively splitting it up into smaller matrices.

    #pragma region Smaller matrices

    struct Matrix2f
    {
    public:

        Matrix2f(const float (&values3x3)[3][3], int ignoreRow, int ignoreCol)
        {
            for (int row = 0; row < 2; ++row)
                for (int col = 0; col < 2; ++col)
                    Get(row, col) = values3x3[row + (row >= ignoreRow ? 1 : 0)]
                                             [col + (col >= ignoreCol ? 1 : 0)];
        }


        float& Get(int row, int col) { return values[row][col]; }
        const float& Get(int row, int col) const { return values[row][col]; }

        float GetDeterminant() const
        {
            return (Get(0, 0) * Get(1, 1)) - (Get(1, 0) * Get(0, 1));
        }

    private:

        float values[2][2];
    };

    struct Matrix3f
    {
    public:

        Matrix3f(const float (&values4x4)[4][4], int ignoreRow, int ignoreCol)
        {
            for (int row = 0; row < 3; ++row)
                for (int col = 0; col < 3; ++col)
                    Get(row, col) = values4x4[row + (row >= ignoreRow ? 1 : 0)]
                                             [col + (col >= ignoreCol ? 1 : 0)];
        }


        float& Get(int row, int col) { return values[row][col]; }
        const float& Get(int row, int col) const { return values[row][col]; }

        float GetDeterminant() const
        {
            //Choose any arbitrary row/column to compute the determinant.
            //Here, we chose the top row.
            bool negative = false;
            const int row = 0;
            float det = 0.0f;
            for (int col = 0; col < 3; ++col)
            {
                float val = Get(row, col);
                if (val != 0.0f)
                {
                    det += val * (negative ? -1 : 1) * Matrix2f(values, row, col).GetDeterminant();
                }
                negative = !negative;
            }
            return det;
        }

    private:

        float values[3][3];
    };

    #pragma endregion
}
using namespace TEMP_MATS;


Matrix4f::Matrix4f(const Matrix4f& one, const Matrix4f& two)
{
    for (int row = 0; row < 4; ++row)
    {
        for (int col = 0; col < 4; ++col)
        {
            Get(row, col) = 0.0f;
            for (int i = 0; i < 4; ++i)
                Get(row, col) += (two.Get(row, i) * one.Get(i, col));
        }
    }
}

float Matrix4f::GetDeterminant() const
{
    //For this algorithm, we can choose an arbitrary row/column to use for computing the determinant.
    //The bottom row is the most likely to have a lot of 0's, so we'll use it.

    bool negative = true;
    const int row = 3;
    float det = 0.0f;
    for (int col = 0; col < 4; ++col)
    {
        float val = Get(row, col);

        //This is an expensive operation; skip it if it will come out to 0.
        if (val != 0.0f)
        {
            det += val * (negative ? -1 : 1) * Matrix3f(values, row, col).GetDeterminant();
        }

        negative = !negative;
    }

    return det;
}
bool Matrix4f::GetInverse(Matrix4f& outM) const
{
    float det = GetDeterminant();
    if (det == 0.0f)
        return false;

    float invDet = 1.0f / det;

    //No easy way to do this :( .
    //https://raw.githubusercontent.com/heyx3/Manbil/master/Manbil/Math/Lower%20Math/Matrix4f.cpp
#define SETM(x0, y0, a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, u, v, w, x, y, z, aa, ab, ac, ad, ae, af, ag, ah, ai, aj) \
    outM.Get(y0, x0) = invDet * ((Get(b, a) * Get(d, c) * Get(f, e)) + \
                                 (Get(h, g) * Get(j, i) * Get(l, k)) + \
                                 (Get(n, m) * Get(p, o) * Get(r, q)) - \
                                 (Get(t, s) * Get(v, u) * Get(x, w)) - \
                                 (Get(z, y) * Get(ab, aa) * Get(ad, ac)) - \
                                 (Get(af, ae) * Get(ah, ag) * Get(aj, ai)));
    SETM(0, 0, 1, 1, 2, 2, 3, 3, 2, 1, 3, 2, 1, 3, 3, 1, 1, 2, 2, 3, 1, 1, 3, 2, 2, 3, 2, 1, 1, 2, 3, 3, 3, 1, 2, 2, 1, 3)
    SETM(1, 0, 1, 0, 3, 2, 2, 3, 2, 0, 1, 2, 3, 3, 3, 0, 2, 2, 1, 3, 1, 0, 2, 2, 3, 3, 2, 0, 3, 2, 1, 3, 3, 0, 1, 2, 2, 3)
    SETM(2, 0, 1, 0, 2, 1, 3, 3, 2, 0, 3, 1, 1, 3, 3, 0, 1, 1, 2, 3, 0, 1, 3, 1, 2, 3, 2, 0, 1, 1, 3, 3, 3, 0, 2, 1, 1, 3)
    SETM(3, 0, 1, 0, 3, 1, 2, 2, 2, 0, 1, 1, 3, 2, 3, 0, 2, 1, 1, 2, 1, 0, 2, 1, 3, 2, 2, 0, 3, 1, 1, 2, 3, 0, 1, 1, 2, 2)
    SETM(0, 1, 0, 1, 3, 2, 2, 3, 2, 1, 0, 2, 3, 3, 3, 1, 2, 2, 0, 3, 0, 1, 2, 2, 3, 3, 2, 1, 3, 2, 0, 3, 3, 1, 0, 2, 2, 3)
    SETM(1, 1, 0, 0, 2, 2, 3, 3, 2, 0, 3, 2, 0, 3, 3, 0, 0, 2, 2, 3, 0, 0, 3, 2, 2, 3, 2, 0, 0, 2, 3, 3, 3, 0, 2, 2, 0, 3)
    SETM(2, 1, 0, 0, 3, 1, 2, 3, 2, 0, 0, 1, 3, 3, 3, 0, 2, 1, 0, 3, 0, 0, 2, 1, 3, 3, 2, 0, 3, 1, 0, 3, 3, 0, 0, 1, 2, 3)
    SETM(3, 1, 0, 0, 2, 1, 3, 2, 2, 0, 3, 1, 0, 2, 3, 0, 0, 1, 2, 2, 0, 0, 3, 1, 2, 2, 2, 0, 0, 1, 3, 2, 3, 0, 2, 1, 0, 2)
    SETM(0, 2, 0, 1, 1, 2, 3, 3, 1, 1, 3, 2, 0, 3, 3, 1, 0, 2, 1, 3, 0, 1, 3, 2, 1, 3, 1, 1, 0, 2, 3, 3, 3, 1, 1, 2, 0, 3)
    SETM(1, 2, 0, 0, 3, 2, 1, 3, 1, 0, 0, 2, 3, 3, 3, 0, 1, 2, 0, 3, 0, 0, 1, 2, 3, 3, 1, 0, 3, 2, 0, 3, 3, 0, 0, 2, 1, 3)
    SETM(2, 2, 0, 0, 1, 1, 3, 3, 1, 0, 3, 1, 0, 3, 3, 0, 0, 1, 1, 3, 0, 0, 3, 1, 1, 3, 1, 0, 0, 1, 3, 3, 3, 0, 1, 1, 0, 3)
    SETM(3, 2, 0, 0, 3, 1, 1, 2, 1, 0, 0, 1, 3, 2, 3, 0, 1, 1, 0, 2, 0, 0, 1, 1, 3, 2, 1, 0, 3, 1, 0, 2, 3, 0, 0, 1, 1, 2)
    SETM(0, 3, 0, 1, 2, 2, 1, 3, 1, 1, 0, 2, 2, 3, 2, 1, 1, 2, 0, 3, 0, 1, 1, 2, 2, 3, 1, 1, 2, 2, 0, 3, 2, 1, 0, 2, 1, 3)
    SETM(1, 3, 0, 0, 1, 2, 2, 3, 1, 0, 2, 2, 0, 3, 2, 0, 0, 2, 1, 3, 0, 0, 2, 2, 1, 3, 1, 0, 0, 2, 2, 3, 2, 0, 1, 2, 0, 3)
    SETM(2, 3, 0, 0, 2, 1, 1, 3, 1, 0, 0, 1, 2, 3, 2, 0, 1, 1, 0, 3, 0, 0, 1, 1, 2, 3, 1, 0, 2, 1, 0, 3, 2, 0, 0, 1, 1, 3)
    SETM(3, 3, 0, 0, 1, 1, 2, 2, 1, 0, 2, 1, 0, 2, 2, 0, 0, 1, 1, 2, 0, 0, 2, 1, 1, 2, 1, 0, 0, 1, 2, 2, 2, 0, 1, 1, 0, 2)
#undef SETM

    return true;
}
void Matrix4f::GetTranspose(Matrix4f& outM) const
{
    for (int row = 0; row < 4; ++row)
        for (int col = 0; col < 4; ++col)
            outM.Get(row, col) = Get(col, row);
}

Vector3f Matrix4f::ApplyPoint(const Vector3f& p) const
{
    float inV[4] = { p.x, p.y, p.z, 1.0f };
    float outV[4] = { 0.0f, 0.0f, 0.0f, 0.0f };

    for (int row = 0; row < 4; ++row)
    {
        for (int col = 0; col < 4; ++col)
            outV[row] += Get(row, col) * inV[col];
    }

    float invW = 1.0f / outV[3];
    return Vector3f(outV[0] * invW, outV[1] * invW, outV[2] * invW);
}
Vector3f Matrix4f::ApplyVector(const Vector3f& p) const
{
    float inV[3] = { p.x, p.y, p.z };
    float outV[4] = { 0.0f, 0.0f, 0.0f, 0.0f };

    //Since w is set to 0.0, we can skip the last column of the matrix.
    for (int row = 0; row < 4; ++row)
        for (int col = 0; col < 3; ++col)
            outV[row] += Get(row, col) * inV[col];
    
    return Vector3f(outV[0], outV[1], outV[2]);
}

void Matrix4f::SetAsIdentity()
{
    Get(0, 0) = 1.0f;   Get(0, 1) = 0.0f;   Get(0, 2) = 0.0f;   Get(0, 3) = 0.0f;
    Get(1, 0) = 0.0f;   Get(1, 1) = 1.0f;   Get(1, 2) = 0.0f;   Get(1, 3) = 0.0f;
    Get(2, 0) = 0.0f;   Get(2, 1) = 0.0f;   Get(2, 2) = 1.0f;   Get(2, 3) = 0.0f;
    Get(3, 0) = 0.0f;   Get(3, 1) = 0.0f;   Get(3, 2) = 0.0f;   Get(3, 3) = 1.0f;
}
void Matrix4f::SetAsScale(const Vector3f& scale)
{
    SetAsIdentity();
    Get(0, 0) = scale.x;
    Get(1, 1) = scale.y;
    Get(2, 2) = scale.z;
}
void Matrix4f::SetAsRotateX(float rad)
{
    float sinA = sinf(rad),
          cosA = cosf(rad);
    SetAsIdentity();
    Get(1, 1) = cosA;
    Get(1, 2) = -sinA;
    Get(2, 1) = sinA;
    Get(2, 2) = cosA;
}
void Matrix4f::SetAsRotateY(float rad)
{
    float sinA = sinf(rad),
          cosA = cosf(rad);
    SetAsIdentity();
    Get(0, 0) = cosA;
    Get(0, 2) = -sinA;
    Get(2, 0) = sinA;
    Get(2, 2) = cosA;
}
void Matrix4f::SetAsRotateZ(float rad)
{
    float sinA = sinf(rad),
          cosA = cosf(rad);
    SetAsIdentity();
    Get(0, 0) = cosA;
    Get(0, 1) = -sinA;
    Get(1, 1) = cosA;
    Get(1, 0) = sinA;
}
void Matrix4f::SetAsRotate(const Quaternion& q)
{
    float x2 = q.x * q.x,
          y2 = q.y * q.y,
          z2 = q.z * q.z,
          w2 = q.w * q.w,
          xy = q.x * q.y,
          xz = q.x * q.z,
          yz = q.y * q.z,
          wx = q.w * q.x,
          wy = q.w * q.y,
          wz = q.w * q.z;
    Get(0, 0) = (w2 + x2 - y2 - z2);  Get(0, 1) = 2.0f * (xy - wz);    Get(0, 2) = 2.0f * (xz + wy);    Get(0, 3) = 0.0f;
    Get(1, 0) = 2.0f * (xy + wz);     Get(1, 1) = (w2 - x2 + y2 - z2); Get(1, 2) = 2.0f * (yz - wx);    Get(1, 3) = 0.0f;
    Get(2, 0) = 2.0f * (xz - wy);     Get(2, 1) = 2.0f * (yz + wx);    Get(2, 2) = (w2 - x2 - y2 + z2); Get(2, 3) = 0.0f;
    Get(3, 0) = 0.0f;                 Get(3, 1) = 0.0f;                Get(3, 2) = 0.0f;                Get(3, 3) = 1.0f;
}
void Matrix4f::SetAsOrientation(const Vector3f& forward, const Vector3f& up, const Vector3f& right)
{
    Get(0, 0) = right.x;    Get(0, 1) = right.y;    Get(0, 2) = right.z;    Get(0, 3) = 0.0f;
    Get(1, 0) = up.x;       Get(1, 1) = up.y;       Get(1, 2) = up.z;       Get(1, 3) = 0.0f;
    Get(2, 0) = forward.x;  Get(2, 1) = forward.y;  Get(2, 2) = forward.z;  Get(2, 3) = 0.0f;
    Get(3, 0) = 0.0f;       Get(3, 1) = 0.0f;       Get(3, 2) = 0.0f;       Get(3, 3) = 1.0f;
}
void Matrix4f::SetAsTranslate(const Vector3f& pos)
{
    SetAsIdentity();
    Get(0, 3) = pos.x;
    Get(1, 3) = pos.y;
    Get(2, 3) = pos.z;
}
void Matrix4f::SetAsView(const Vector3f& camPos, const Vector3f& camForward,
                         const Vector3f& camUp, const Vector3f& camRight)
{
    Matrix4f transM, rotM;
    transM.SetAsTranslate(-camPos);
    rotM.SetAsOrientation(camForward, camUp, camRight);

    *this = Matrix4f(transM, rotM);
}
void Matrix4f::SetAsOrthoProjection(const Vector3f& minP, const Vector3f& maxP)
{
    Vector3f invSize = (maxP - minP).Reciprocal();
    Get(0, 0) = 2.0f * invSize.x;   Get(0, 1) = 0.0f;               Get(0, 2) = 0.0f;               Get(0, 3) = -(maxP.x + minP.x) * invSize.x;
    Get(1, 0) = 0.0f;               Get(1, 1) = 2.0f * invSize.y;   Get(1, 2) = 0.0f;               Get(1, 3) = -(maxP.y + minP.y) * invSize.y;
    Get(2, 0) = 0.0f;               Get(2, 1) = 0.0f;               Get(2, 2) = 2.0f * invSize.z;   Get(1, 3) = -(maxP.z + minP.z) * invSize.z;
    Get(3, 0) = 0.0f;               Get(3, 1) = 0.0f;               Get(3, 2) = 0.0f;               Get(3, 3) = 1.0f;
}
void Matrix4f::SetAsPerspectiveProjection(float fovRadians, float widthOverHeight,
                                          float zNear, float zFar)
{
    float invZRange = 1.0f / (zNear - zFar),
          tanHalfFOV = tanf(fovRadians * 0.5f);
    Get(0, 0) = 1.0f / (tanHalfFOV * widthOverHeight);  Get(0, 1) = 0.0f;              Get(0, 2) = 0.0f;                        Get(0, 3) = 0.0f;
    Get(1, 0) = 0.0f;                                   Get(1, 1) = 1.0f / tanHalfFOV; Get(1, 2) = 0.0f;                        Get(1, 3) = 0.0f;     
    Get(2, 0) = 0.0f;                                   Get(2, 1) = 0.0f;              Get(2, 2) = -(zNear + zFar) * invZRange; Get(2, 3) = 2.0f * zFar * zNear / invZRange;
    Get(3, 0) = 0.0f;                                   Get(3, 1) = 0.0f;              Get(3, 2) = 1.0f;                        Get(3, 3) = 0.0f;
}
void Matrix4f::SetAsWVP(const Matrix4f& worldM, const Matrix4f& viewM, const Matrix4f& projM)
{
    *this = Matrix4f(worldM, Matrix4f(viewM, projM));
}