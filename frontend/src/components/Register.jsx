import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';

const Register = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [email, setEmail] = useState('');
  const [birthdate, setBirthdate] = useState('');
  const navigate = useNavigate();

  const handleRegister = async (e) => {
    e.preventDefault();
    try {
      const response = await fetch('http://localhost:5001/auth/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ username, password, email, birthdate }),
      });

      if (response.ok) {
        alert('Registro exitoso. Ahora inicia sesión.');
        navigate('/login');
      } else {
        alert('El registro falló. Verifica los datos ingresados.');
      }
    } catch (error) {
      console.error('Error during registration:', error);
      alert('Ocurrió un error durante el registro.');
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-header">
          <h2>Crea tu cuenta</h2>
          <p>Regístrate para acceder a tu panel de usuario personalizado.</p>
        </div>

        <form onSubmit={handleRegister}>
          <div className="input-group">
            <label>Nombre</label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="Nombre de usuario"
              required
            />
          </div>

          <div className="input-group">
            <label>Correo</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="tu@correo.com"
              required
            />
          </div>

          <div className="input-row">
            <div className="input-group">
              <label>Fecha de nacimiento</label>
              <input
                type="date"
                value={birthdate}
                onChange={(e) => setBirthdate(e.target.value)}
                required
              />
            </div>

            <div className="input-group">
              <label>Contraseña</label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="********"
                required
              />
            </div>
          </div>

          <button className="auth-button" type="submit">
            Registrar
          </button>
        </form>

        <div className="auth-footer">
          <p>¿Ya tienes una cuenta?</p>
          <Link to="/login">Inicia sesión aquí</Link>
        </div>
      </div>
    </div>
  );
};

export default Register;

