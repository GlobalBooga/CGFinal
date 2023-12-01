using OpenTK.Mathematics;


namespace SpaceShooter
{
    public class Transform
    {
        private Vector3 _position;
        private Vector3 _rotation;
        private Vector3 _scale;

        public GameObject? parent { get; private set; }

        public Transform(GameObject? parent = null)
        {
            GetMatrix = Matrix4.Identity;
            _position = Vector3.Zero;
            _rotation = Vector3.Zero;
            _scale = Vector3.One;
            this.parent = parent;
        }

        public Vector3 Position 
        { 
            get => _position;
            set 
            {
                _position = value;
                UpdateMatrix();
            } 
        }
        public Vector3 Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                UpdateMatrix();
            }
        }
        public Vector3 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                UpdateMatrix();
            }
        }

        public Matrix4 GetMatrix;

        private void UpdateMatrix()
        {
            GetMatrix = Matrix4.Identity;

            GetMatrix *= Matrix4.CreateScale(_scale);

            GetMatrix *= Matrix4.CreateRotationX(_rotation.X);
            GetMatrix *= Matrix4.CreateRotationY(_rotation.Y);
            GetMatrix *= Matrix4.CreateRotationZ(_rotation.Z);

            GetMatrix *= Matrix4.CreateTranslation(_position);
        }
    }
}
