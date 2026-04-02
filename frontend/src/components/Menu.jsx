import { useState } from 'react';
import { Link } from 'react-router-dom';

const tabs = ['Perfil', 'Citas', 'Ajustes', 'Cerrar sesión'];

const Menu = () => {
  const [activeTab, setActiveTab] = useState('Perfil');

  return (
    <div
      style={{
        minHeight: '100vh',
        backgroundImage: 'url(/background.jpg)',
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        padding: '3rem',
        color: '#fff',
      }}
    >
      <div
        style={{
          maxWidth: '1000px',
          margin: '0 auto',
          background: 'rgba(0, 0, 0, 0.65)',
          borderRadius: '20px',
          padding: '2rem',
          boxShadow: '0 0 40px rgba(0,0,0,0.45)',
        }}
      >
        <header style={{ marginBottom: '2rem' }}>
          <h1 style={{ margin: 0, fontSize: '2.6rem' }}>Menú de usuario</h1>
          <p style={{ color: '#ccc', marginTop: '0.8rem' }}>
            Selecciona una opción en las pestañas para ver las próximas páginas.
          </p>
        </header>

        <nav style={{ display: 'flex', gap: '0.75rem', flexWrap: 'wrap', marginBottom: '2rem' }}>
          {tabs.map((tab) => (
            <button
              key={tab}
              type="button"
              onClick={() => setActiveTab(tab)}
              style={{
                flex: 1,
                minWidth: '140px',
                padding: '1rem 1.25rem',
                borderRadius: '12px',
                border: activeTab === tab ? '2px solid #fff' : '1px solid rgba(255,255,255,0.35)',
                background: activeTab === tab ? '#fff' : 'rgba(255,255,255,0.08)',
                color: activeTab === tab ? '#000' : '#fff',
                cursor: 'pointer',
                fontWeight: 600,
              }}
            >
              {tab}
            </button>
          ))}
        </nav>

        <section style={{ padding: '1.5rem', background: 'rgba(255,255,255,0.08)', borderRadius: '16px' }}>
          <h2 style={{ marginTop: 0 }}>{activeTab}</h2>
          <p style={{ color: '#ddd', lineHeight: 1.8 }}>
            Esta es un área de contenido placeholder para la pestaña <strong>{activeTab}</strong>.
            Aquí podrás añadir los componentes específicos del usuario más adelante.
          </p>
          <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', marginTop: '1.5rem' }}>
            <div style={{ flex: 1, minWidth: '220px', padding: '1rem', background: 'rgba(255,255,255,0.12)', borderRadius: '12px' }}>
              <h3>Próximamente</h3>
              <p>Tablas, formularios y páginas específicas para cada módulo.</p>
            </div>
            <div style={{ flex: 1, minWidth: '220px', padding: '1rem', background: 'rgba(255,255,255,0.12)', borderRadius: '12px' }}>
              <h3>Personalización</h3>
              <p>Podrás cambiar la imagen de fondo y el contenido según tu estilo.</p>
            </div>
          </div>
        </section>

        <footer style={{ marginTop: '2rem', color: '#bbb' }}>
          <Link to="/login" style={{ color: '#fff', textDecoration: 'underline' }}>
            Volver a iniciar sesión
          </Link>
        </footer>
      </div>
    </div>
  );
};

export default Menu;
