#pragma once

#include "Quaternion.h"


namespace RT
{
    struct RT_API Matrix4f
    {
    public:

        Matrix4f() { SetAsIdentity(); }
        Matrix4f(float (&rowThenCol)[4][4]) { memcpy(values, rowThenCol, sizeof(float) * 16); }
        Matrix4f(const Matrix4f& firstTransform, const Matrix4f& secondTransform);


        float& Get(int row, int col) { return values[row][col]; }
        const float& Get(int row, int col) const { return values[row][col]; }


        float GetDeterminant() const;
        void GetTranspose(Matrix4f& outM) const;
    
        //Returns whether the inverse was able to be computed.
        bool GetInverse(Matrix4f& outM) const;

        Vector3f ApplyPoint(const Vector3f& p) const;
        Vector3f ApplyVector(const Vector3f& v) const;

        //Applies the given transformation on top of this one.
        void ApplyTransformation(const Matrix4f& mat) { *this = Matrix4f(*this, mat); }

        void SetAsIdentity();
        void SetAsScale(float scale) { SetAsScale(Vector3f(scale, scale, scale)); }
        void SetAsScale(const Vector3f& scale);
        void SetAsRotateX(float radians);
        void SetAsRotateY(float radians);
        void SetAsRotateZ(float radians);
        void SetAsRotate(const Quaternion& rot);
        void SetAsTranslate(const Vector3f& amount);
        void SetAsOrientation(const Vector3f& forward, const Vector3f& up, const Vector3f& right);
        void SetAsOrthoProjection(const Vector3f& boundsMin, const Vector3f& boundsMax);
        void SetAsPerspectiveProjection(float fovRadians, float widthOverHeight, float zNear, float zFar);
        void SetAsView(const Vector3f& camPos, const Vector3f& camForward,
                       const Vector3f& camUp, const Vector3f& camRight);
        void SetAsWVP(const Matrix4f& worldM, const Matrix4f& viewM, const Matrix4f& projM);

    private:

        float values[4][4];
    };
}