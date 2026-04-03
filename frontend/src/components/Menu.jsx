import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';

const pestañas = ['Perfil', 'Citas', 'Ajustes', 'Cerrar sesión'];

const Menu = () => {
  const [pestañaActiva, setPestañaActiva] = useState('Perfil');
  const [perfil, setPerfil] = useState({ id: '', nombreUsuario: '', correo: '', fechaNacimiento: '' });
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [fechaNacimiento, setFechaNacimiento] = useState('');
  const [mensajeEstado, setMensajeEstado] = useState('');
  const [cargando, setCargando] = useState(false);

  const [citas, setCitas] = useState([]);
  const [citasFiltradas, setCitasFiltradas] = useState([]);
  const [mensajeCitas, setMensajeCitas] = useState('');
  const [citaEspecialidadActual, setCitaEspecialidadActual] = useState(null);

  const [especialidades, setEspecialidades] = useState([]);
  const [medicosFiltrados, setMedicosFiltrados] = useState([]);
  const [especialidadSeleccionada, setEspecialidadSeleccionada] = useState('');
  const [filtroEstadoCita, setFiltroEstadoCita] = useState('Pendiente');
  const [filtroFechaCita, setFiltroFechaCita] = useState('');
  const [filtroEspecialidadCita, setFiltroEspecialidadCita] = useState('');

  const [medicoSeleccionado, setMedicoSeleccionado] = useState('');
  const [citasDisponibles, setCitasDisponibles] = useState([]);
  const [paginaActual, setPaginaActual] = useState(1);
  const citasPorPagina = 10;

  // Estados para la pestaña de Ajustes (solo médicos)
  const [esMedico, setEsMedico] = useState(false);
  const [medicoInfo, setMedicoInfo] = useState(null);
  const [disponibilidades, setDisponibilidades] = useState([]);
  const [fechaDisponibilidad, setFechaDisponibilidad] = useState('');
  const [horaInicio, setHoraInicio] = useState('09:00');
  const [horaFin, setHoraFin] = useState('17:00');
  const [duracionMinutos, setDuracionMinutos] = useState(30);
  const [mensajeAjustes, setMensajeAjustes] = useState('');
  const [cargandoAjustes, setCargandoAjustes] = useState(false);
  const [citasMedicoReporte, setCitasMedicoReporte] = useState([]);
  const [filtroFechaInicio, setFiltroFechaInicio] = useState('');
  const [filtroFechaFin, setFiltroFechaFin] = useState('');
  const [filtroPaciente, setFiltroPaciente] = useState('');
  const [filtroEstado, setFiltroEstado] = useState('');
  const [mensajeReporte, setMensajeReporte] = useState('');


  const navigate = useNavigate();

  const token = localStorage.getItem('token');

  useEffect(() => {
    // Redirección forzada si no hay token al cargar el componente
    if (!token) {
      console.log('No hay token, redirigiendo al login...');
      navigate('/login');
    }
  }, [token, navigate]);

  const cargarPerfil = async () => {
    if (!token) {
      setMensajeEstado('No se encontró token de sesión. Inicia sesión otra vez.');
      return;
    }

    setCargando(true);
    setMensajeEstado('Cargando perfil...');

    try {
      const respuesta = await fetch('http://localhost:5001/auth/me', {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
      });

      if (!respuesta.ok) {
        if (respuesta.status === 401) {
          setMensajeEstado('Sesión expirada. Por favor ingresa nuevamente.');
          localStorage.removeItem('token');
          navigate('/login');
          return;
        }

        const textoError = await respuesta.text();
        setMensajeEstado(`Error al cargar perfil: ${textoError}`);
        return;
      }

      const datos = await respuesta.json();
      setPerfil({
        id: datos.id ?? '',
        nombreUsuario: datos.username ?? '',
        correo: datos.email ?? '',
        fechaNacimiento: datos.birthdate ? new Date(datos.birthdate).toISOString().slice(0, 10) : '',
      });

      setEmail(datos.email ?? '');
      setFechaNacimiento(datos.birthdate ? new Date(datos.birthdate).toISOString().slice(0, 10) : '');
      setPassword('');
      setMensajeEstado('Perfil cargado correctamente.');
    } catch (error) {
      console.error('Error cargando perfil:', error);
      setMensajeEstado('Error de conexión al obtener perfil.');
    } finally {
      setCargando(false);
    }
  };

  const actualizarPerfil = async (event) => {
    event.preventDefault();

    if (!token) {
      setMensajeEstado('No se encontró token de sesión. Inicia sesión otra vez.');
      return;
    }

    setCargando(true);
    setMensajeEstado('Guardando cambios...');

    try {
      const respuesta = await fetch('http://localhost:5001/auth/me', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          email,
          password,
          birthdate: fechaNacimiento || null,
        }),
      });

      if (!respuesta.ok) {
        const textoError = await respuesta.text();
        setMensajeEstado(`Fallo al actualizar perfil: ${textoError}`);
        return;
      }

      const datosActualizados = await respuesta.json();
      setPerfil({
        nombreUsuario: datosActualizados.username ?? perfil.nombreUsuario,
        correo: datosActualizados.email ?? email,
        fechaNacimiento: datosActualizados.birthdate ? new Date(datosActualizados.birthdate).toISOString().slice(0, 10) : fechaNacimiento,
      });
      setMensajeEstado('Perfil actualizado con éxito.');
      setPassword('');
    } catch (error) {
      console.error('Error actualizando perfil:', error);
      setMensajeEstado('Error de conexión al actualizar perfil.');
    } finally {
      setCargando(false);
    }
  };

  const cargarCitas = async () => {
    if (!token) {
      setMensajeCitas('Debes iniciar sesión para ver citas.');
      return;
    }

    try {
      const respuesta = await fetch('http://localhost:5001/citas', {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
      });

      if (!respuesta.ok) {
        setMensajeCitas('Error al obtener citas.');
        return;
      }

      const datos = await respuesta.json();
      setCitas(datos);
      setMensajeCitas('Citas cargadas');
    } catch (error) {
      console.error('Error cargando citas:', error);
      setMensajeCitas('Error de conexión al cargar citas.');
    }
  };

  const cargarEspecialidades = async () => {
    if (!token) return;

    try {
      const respuesta = await fetch('http://localhost:5001/citas/especialidades', {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
      });

      if (!respuesta.ok) {
        setMensajeCitas('Error al obtener especialidades.');
        console.error('Error en cargarEspecialidades:', respuesta.status, respuesta.statusText);
        return;
      }

      const datos = await respuesta.json();
      console.log('Especialidades cargadas:', datos);
      setEspecialidades(datos);
      if (datos.length > 0 && !especialidadSeleccionada) {
        setEspecialidadSeleccionada(datos[0].id);
      } else if (datos.length === 0) {
        setMensajeCitas('No hay especialidades disponibles.');
      }
    } catch (error) {
      console.error('Error cargando especialidades:', error);
      setMensajeCitas('Error de conexión al cargar especialidades.');
    }
  };

  const filtrarMedicos = async (especialidadId) => {
    if (!token || !especialidadId) {
      setMedicosFiltrados([]);
      setMedicoSeleccionado('');
      return;
    }

    try {
      const respuesta = await fetch(`http://localhost:5001/citas/medicos-por-especialidad?especialidadId=${especialidadId}`, {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
      });

      if (!respuesta.ok) {
        setMensajeCitas('Error al filtrar médicos.');
        console.error('Error en filtrarMedicos:', respuesta.status, respuesta.statusText);
        return;
      }

      const datos = await respuesta.json();
      console.log('Médicos filtrados:', datos);
      setMedicosFiltrados(datos);
      if (datos.length > 0 && !medicoSeleccionado) {
        setMedicoSeleccionado(datos[0].id);
      } else if (datos.length === 0) {
        setMedicoSeleccionado('');
        setMensajeCitas('No hay médicos disponibles para esta especialidad.');
      }
    } catch (error) {
      console.error('Error filtrando médicos:', error);
      setMensajeCitas('Error de conexión al filtrar médicos.');
    }
  };

  const buscarCitasDisponibles = async () => {
    if (!especialidadSeleccionada) {
      setMensajeCitas('Selecciona una especialidad.');
      return;
    }

    if (!medicoSeleccionado) {
      setMensajeCitas('Selecciona un médico.');
      return;
    }

    // Cargar citas del usuario y citas ocupadas del médico en paralelo
    let citasDelUsuario = [];
    let citasOcupadasDelMedico = [];
    try {
      const [resCitasUsuario, resCitasOcupadas] = await Promise.all([
        fetch('http://localhost:5001/citas', { headers: { Authorization: `Bearer ${token}` } }),
        fetch(`http://localhost:5001/citas/citas-ocupadas?medicoId=${medicoSeleccionado}`, { headers: { Authorization: `Bearer ${token}` } })
      ]);

      if (resCitasUsuario.ok) {
        citasDelUsuario = await resCitasUsuario.json();
        setCitas(citasDelUsuario);
      }
      if (resCitasOcupadas.ok) {
        citasOcupadasDelMedico = await resCitasOcupadas.json();
      }
    } catch (error) {
      console.error('Error cargando datos de citas:', error);
    }

    try {
      const respuesta = await fetch(`http://localhost:5001/citas/disponibilidad?medicoId=${medicoSeleccionado}`, {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
      });

      if (!respuesta.ok) {
        setMensajeCitas('Error al obtener disponibilidad.');
        return;
      }

      const disponibilidad = await respuesta.json();
      console.log('Disponibilidad obtenida:', disponibilidad);

      // Calcular citas disponibles de forma robusta
      const disponibles = [];
      const hoy = new Date();
      hoy.setHours(0, 0, 0, 0); // Normalizar a medianoche

      for (let i = 1; i <= 30; i++) { // Próximos 30 días
        const fecha = new Date(hoy);
        fecha.setDate(hoy.getDate() + i);
        const fechaStr = fecha.toISOString().slice(0, 10);

        const dispDia = disponibilidad.find(d => (d.fechaDisponibilidad || '').slice(0, 10) === fechaStr);
        if (dispDia) {
          const [hInicio, mInicio] = dispDia.horaInicio.split(':').map(Number);
          const [hFin, mFin] = dispDia.horaFin.split(':').map(Number);

          let minutosActuales = hInicio * 60 + mInicio;
          const minutosFin = hFin * 60 + mFin;

          while (minutosActuales + dispDia.duracionCitaMinutos <= minutosFin) {
            const hora = Math.floor(minutosActuales / 60).toString().padStart(2, '0');
            const minutos = (minutosActuales % 60).toString().padStart(2, '0');
            const horaStr = `${hora}:${minutos}:00`; // Formato HH:mm:ss

            disponibles.push({
              fecha: fechaStr,
              hora: horaStr,
              medicoId: medicoSeleccionado,
              medicoNombre: medicosFiltrados.find(m => m.id === medicoSeleccionado)?.nombre + ' ' + medicosFiltrados.find(m => m.id === medicoSeleccionado)?.apellido,
              especialidad: especialidades.find(e => e.id === especialidadSeleccionada)?.nombre,
            });
            minutosActuales += dispDia.duracionCitaMinutos;
          }
        }
      }

      const citasTomadasSet = new Set(citasOcupadasDelMedico.map(c => `${(c.fechaCita || '').slice(0, 10)}T${(c.horaCita || '')}`));
      const disponiblesFiltradas = disponibles.filter(d => !citasTomadasSet.has(`${d.fecha}T${d.hora}`));

      // Ver si hay una cita activa (no cancelada) en esta especialidad para este paciente
      const nombreEspecialidadSeleccionada = especialidades.find(e => e.id === especialidadSeleccionada)?.nombre;
      const citaEspecial = citasDelUsuario.find(c => c.especialidad === nombreEspecialidadSeleccionada && c.estado !== 'Cancelada');
      console.log('Cita especial encontrada:', citaEspecial);
      setCitaEspecialidadActual(citaEspecial || null);

      setCitasDisponibles(disponiblesFiltradas);
      setPaginaActual(1);
      setMensajeCitas(citaEspecial ? 'Ya tienes una cita PENDIENTE para esta especialidad. Puedes reprogramarla seleccionando otro horario.' : 'Citas disponibles cargadas.');
    } catch (error) {
      console.error('Error buscando citas disponibles:', error);
      setMensajeCitas('Error de conexión al buscar citas disponibles.');
    }
  };

  const seleccionarCita = async (citaDisp) => {
    if (!token || !perfil.id) {
      setMensajeCitas('Debes iniciar sesión.');
      return;
    }

    const esReprogramacion = !!citaEspecialidadActual;

    if (esReprogramacion) {
      const confirmar = window.confirm('Ya tienes una cita en esta especialidad. ¿Deseas reprogramarla con el nuevo horario?');
      if (!confirmar) {
        setMensajeCitas('Reprogramación cancelada.');
        return;
      }
    }

    setMensajeCitas(esReprogramacion ? 'Reprogramando cita...' : 'Creando cita...');

    try {
      const payload = {
        idMedico: citaDisp.medicoId,
        idPaciente: perfil.id,
        fechaCita: citaDisp.fecha,
        horaCita: citaDisp.hora.length === 5 ? `${citaDisp.hora}:00` : citaDisp.hora, // Asegurar formato HH:mm:ss
      };

      console.log('Seleccionar cita payload', payload);

      const url = esReprogramacion
        ? `http://localhost:5001/citas/${citaEspecialidadActual.id}`
        : 'http://localhost:5001/citas';
      const method = esReprogramacion ? 'PUT' : 'POST';

      const respuesta = await fetch(url, {
        method,
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(payload),
      });

      if (!respuesta.ok) {
        const errorTexto = await respuesta.text();
        console.error('No se pudo procesar la cita', respuesta.status, errorTexto);
        setMensajeCitas(`No se pudo ${esReprogramacion ? 'reprogramar' : 'crear'} la cita: ${errorTexto}`);
        return;
      }

      const citaResultado = await respuesta.json();
      console.log(esReprogramacion ? 'Cita reprogramada' : 'Cita creada', citaResultado);
      const mensajeExito = esReprogramacion ? 'Cita reprogramada con éxito.' : 'Cita creada con éxito.';
      setMensajeCitas(mensajeExito);

      window.alert(mensajeExito);

      const citaId = citaResultado.id || citaResultado.Id;
      if (citaId) {
        await descargarPdfCita(citaId);
      }

      // Limpiar el formulario de búsqueda y la grilla de disponibles
      setEspecialidadSeleccionada('');
      setMedicoSeleccionado('');
      setMedicosFiltrados([]);
      setCitasDisponibles([]);
      setCitaEspecialidadActual(null);

      await cargarCitas();
    } catch (error) {
      console.error('Error procesando cita:', error);
      setMensajeCitas(`Error de conexión al ${esReprogramacion ? 'reprogramar' : 'crear'} cita.`);
    }
  };

  const descargarPdfCita = async (citaId) => {
    if (!token) {
      setMensajeCitas('Debes iniciar sesión para descargar PDF de la cita.');
      return;
    }

    try {
      const respuesta = await fetch(`http://localhost:5001/citas/${citaId}/pdf`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (!respuesta.ok) {
        console.error('No se pudo descargar el PDF de la cita', respuesta.status);
        return;
      }

      const blob = await respuesta.blob();
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `Cita_${citaId}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);

      setMensajeCitas('PDF descargado correctamente.');
    } catch (error) {
      console.error('Error descargando PDF:', error);
      setMensajeCitas('No se pudo descargar el PDF de la cita.');
    }
  };

  const cancelarCita = async (idCita) => {
    if (!token) {
      setMensajeCitas('Debes iniciar sesión para cancelar una cita.');
      return;
    }

    try {
      const respuesta = await fetch(`http://localhost:5001/citas/${idCita}/cancelar`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
      });

      if (!respuesta.ok) {
        const errorTexto = await respuesta.text();
        setMensajeCitas(`No se pudo cancelar la cita: ${errorTexto}`);
        return;
      }

      setMensajeCitas('Cita cancelada.');
      await cargarCitas();
    } catch (error) {
      console.error('Error cancelando cita:', error);
      setMensajeCitas('Error de conexión al cancelar cita.');
    }
  };

  // FUNCIONES PARA MÉDICOS (PESTAÑA AJUSTES)

  const cargarInfoMedico = async () => {
    if (!token) return;

    try {
      const respuesta = await fetch('http://localhost:5001/citas/medico/me', {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
      });

      if (respuesta.ok) {
        const datos = await respuesta.json();
        setMedicoInfo(datos);
        setEsMedico(true);
        await cargarDisponibilidadesMedico();
        await cargarReporteCitasMedico();
      } else {
        setEsMedico(false);
      }
    } catch (error) {
      console.error('Error cargando info del médico:', error);
      setEsMedico(false);
    }
  };

  const cargarDisponibilidadesMedico = async () => {
    if (!token) return;

    try {
      const respuesta = await fetch('http://localhost:5001/citas/medico/disponibilidades', {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
      });

      if (respuesta.ok) {
        const datos = await respuesta.json();
        setDisponibilidades(datos);
      }
    } catch (error) {
      console.error('Error cargando disponibilidades:', error);
    }
  };

  const crearDisponibilidad = async (e) => {
    e.preventDefault();

    if (!token) {
      setMensajeAjustes('Debes iniciar sesión.');
      return;
    }

    setCargandoAjustes(true);
    setMensajeAjustes('Creando disponibilidad...');

    try {
      const respuesta = await fetch('http://localhost:5001/citas/medico/disponibilidades', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          fechaDisponibilidad,
          horaInicio,
          horaFin,
          duracionCitaMinutos: parseInt(duracionMinutos),
        }),
      });

      if (!respuesta.ok) {
        const errorTexto = await respuesta.text();
        setMensajeAjustes(`Error: ${errorTexto}`);
        return;
      }

      const nuevaDisponibilidad = await respuesta.json();

      setMensajeAjustes('Disponibilidad creada exitosamente.');
      setFechaDisponibilidad('');
      setHoraInicio('09:00');
      setHoraFin('17:00');
      setDuracionMinutos(30);

      setDisponibilidades(prev => [...prev, nuevaDisponibilidad]);
    } catch (error) {
      console.error('Error creando disponibilidad:', error);
      setMensajeAjustes('Error de conexión.');
    } finally {
      setCargandoAjustes(false);
    }
  };

  const eliminarDisponibilidad = async (disponibilidadId) => {
    if (!token) {
      setMensajeAjustes('Debes iniciar sesión.');
      return;
    }

    if (!window.confirm('¿Seguro que deseas eliminar esta disponibilidad?')) {
      return;
    }

    try {
      const respuesta = await fetch(`http://localhost:5001/citas/medico/disponibilidades/${disponibilidadId}`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
      });

      if (!respuesta.ok) {
        const errorTexto = await respuesta.text();
        setMensajeAjustes(`Error: ${errorTexto}`);
        return;
      }

      setMensajeAjustes('Disponibilidad eliminada.');
      await cargarDisponibilidadesMedico();
    } catch (error) {
      console.error('Error eliminando disponibilidad:', error);
      setMensajeAjustes('Error de conexión.');
    }
  };

  const cargarReporteCitasMedico = async () => {
    if (!token) return;

    setMensajeReporte('Cargando reporte...');

    try {
      const params = new URLSearchParams();
      if (filtroFechaInicio) params.append('fechaInicio', filtroFechaInicio);
      if (filtroFechaFin) params.append('fechaFin', filtroFechaFin);
      if (filtroPaciente) params.append('paciente', filtroPaciente);
      if (filtroEstado) params.append('estado', filtroEstado);

      const respuesta = await fetch(`http://localhost:5001/citas/medico/reporte?${params.toString()}`, {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
      });

      if (respuesta.ok) {
        const datos = await respuesta.json();
        setCitasMedicoReporte(datos);
        setMensajeReporte(`Reporte cargado. Total: ${datos.length} citas.`);
      } else {
        const errorTexto = await respuesta.text();
        console.error('Error al cargar el reporte:', respuesta.status, errorTexto);
        setMensajeReporte('Error al cargar el reporte.');
      }
    } catch (error) {
      console.error('Error cargando reporte de citas:', error);
      setMensajeReporte('Error de conexión al cargar el reporte.');
    }
  };

  const descargarReportePdfMedico = async () => {
    if (!token) {
      setMensajeAjustes('Debes iniciar sesión.');
      return;
    }

    try {
      const params = new URLSearchParams();
      if (filtroFechaInicio) params.append('fechaInicio', filtroFechaInicio);
      if (filtroFechaFin) params.append('fechaFin', filtroFechaFin);
      if (filtroPaciente) params.append('paciente', filtroPaciente);
      if (filtroEstado) params.append('estado', filtroEstado);

      const respuesta = await fetch(`http://localhost:5001/citas/medico/reporte/pdf?${params.toString()}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (!respuesta.ok) {
        setMensajeAjustes('Error al descargar el reporte.');
        return;
      }

      const blob = await respuesta.blob();
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `Reporte_Citas_${new Date().toISOString().slice(0, 10)}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);

      setMensajeAjustes('Reporte descargado exitosamente.');
    } catch (error) {
      console.error('Error descargando reporte:', error);
      setMensajeAjustes('Error al descargar el reporte.');
    }
  };

  useEffect(() => {
    if (pestañaActiva === 'Perfil') {
      cargarPerfil();
    } else if (pestañaActiva === 'Citas') {
      cargarEspecialidades();
      cargarCitas();
    } else if (pestañaActiva === 'Ajustes') {
      cargarInfoMedico();
    } else if (pestañaActiva === 'Cerrar sesión') {
      localStorage.removeItem('token');
      navigate('/login');
    }
  }, [pestañaActiva]);

  useEffect(() => {
    if (especialidadSeleccionada) {
      filtrarMedicos(especialidadSeleccionada);
    }
  }, [especialidadSeleccionada]);

  useEffect(() => {
    let citasResultado = [...citas];

    if (filtroEstadoCita) {
      citasResultado = citasResultado.filter(c => c.estado === filtroEstadoCita);
    }
    if (filtroFechaCita) {
      citasResultado = citasResultado.filter(c => (c.fechaCita || '').slice(0, 10) === filtroFechaCita);
    }
    if (filtroEspecialidadCita) {
      citasResultado = citasResultado.filter(c => c.especialidad === filtroEspecialidadCita);
    }

    setCitasFiltradas(citasResultado);
  }, [citas, filtroEstadoCita, filtroFechaCita, filtroEspecialidadCita]);

  const pestañaContenido = () => {
    if (pestañaActiva === 'Perfil') {
      return (
        <form onSubmit={actualizarPerfil} style={{ maxWidth: '520px' }}>
          <div style={{ marginBottom: '1rem' }}>
            <label style={{ display: 'block', marginBottom: '0.4rem' }}>Usuario</label>
            <input
              type="text"
              value={perfil.nombreUsuario}
              disabled
              style={{ width: '100%', padding: '0.7rem', borderRadius: '8px', border: '1px solid #0b0a0a', backgroundColor: '#dfdfdf' }}
            />
          </div>

          <div style={{ marginBottom: '1rem' }}>
            <label style={{ display: 'block', marginBottom: '0.4rem' }}>Correo electrónico</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              style={{ width: '100%', padding: '0.7rem', borderRadius: '8px', border: '1px solid #999' }}
            />
          </div>

          <div style={{ marginBottom: '1rem' }}>
            <label style={{ display: 'block', marginBottom: '0.4rem' }}>Contraseña nueva (dejar vacío para no cambiar)</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="*******"
              style={{ width: '100%', padding: '0.7rem', borderRadius: '8px', border: '1px solid #999' }}
            />
          </div>

          <div style={{ marginBottom: '1.5rem' }}>
            <label style={{ display: 'block', marginBottom: '0.4rem' }}>Fecha de nacimiento</label>
            <input
              type="date"
              value={fechaNacimiento}
              onChange={(e) => setFechaNacimiento(e.target.value)}
              style={{ width: '100%', padding: '0.7rem', borderRadius: '8px', border: '1px solid #999' }}
            />
          </div>

          <button
            type="submit"
            disabled={cargando}
            style={{
              padding: '0.85rem 1.4rem',
              borderRadius: '10px',
              border: 'none',
              backgroundColor: '#4caf50',
              color: '#fff',
              fontWeight: 700,
              cursor: 'pointer',
            }}
          >
            {cargando ? 'Guardando…' : 'Guardar cambios'}
          </button>

          {mensajeEstado && <p style={{ marginTop: '1rem', color: '#ddd' }}>{mensajeEstado}</p>}
        </form>
      );
    }

    if (pestañaActiva === 'Citas') {
      return (
        <div>
          <div style={{ marginBottom: '1rem', display: 'flex', gap: '0.75rem', flexWrap: 'wrap' }}>
            <form style={{ flex: 1, minWidth: '280px', border: '1px solid rgba(255,255,255,0.25)', borderRadius: '12px', padding: '1rem', backgroundColor: 'rgba(0,0,0,0.2)' }}>
              <h3 style={{ marginTop: 0 }}>Buscar citas disponibles</h3>

              <label style={{ display: 'block', marginBottom: '0.4rem' }}>Especialidad</label>
              <select
                value={especialidadSeleccionada}
                onChange={(e) => {
                  setEspecialidadSeleccionada(e.target.value);
                  setMedicoSeleccionado('');
                  setCitasDisponibles([]);
                  setCitaEspecialidadActual(null);
                }}
                required
                style={{ width: '100%', padding: '0.6rem', borderRadius: '8px', border: '1px solid #999', marginBottom: '0.75rem' }}
              >
                <option value="">Selecciona una especialidad</option>
                {especialidades.map((esp) => (
                  <option key={esp.id} value={esp.id}>{esp.nombre}</option>
                ))}
              </select>

              <label style={{ display: 'block', marginBottom: '0.4rem' }}>Médico</label>
              <select
                value={medicoSeleccionado}
                onChange={(e) => setMedicoSeleccionado(e.target.value)}
                disabled={!especialidadSeleccionada || medicosFiltrados.length === 0}
                required
                style={{width: '100%', padding: '0.6rem', borderRadius: '8px', border: '1px solid #999', marginBottom: '0.75rem' 
                }}
              >
                <option value="">
                  {especialidadSeleccionada ? (medicosFiltrados.length === 0 ? 'No hay médicos disponibles' : 'Selecciona un médico') : 'Primero selecciona especialidad'}
                </option>
                {medicosFiltrados.map((med) => (
                  <option key={med.id} value={med.id}>{`${med.nombre} ${med.apellido}`}</option>
                ))}
              </select>

              <button
                type="button"
                onClick={buscarCitasDisponibles}
                disabled={!especialidadSeleccionada || !medicoSeleccionado}
                style={{
                  width: '100%',
                  marginTop: '0.5rem',
                  padding: '0.75rem',
                  borderRadius: '10px',
                  border: 'none',
                  backgroundColor: (!especialidadSeleccionada || !medicoSeleccionado) ? '#ccc' : '#2196f3',
                  color: '#fff',
                  fontWeight: 700,
                  cursor: (!especialidadSeleccionada || !medicoSeleccionado) ? 'not-allowed' : 'pointer',
                }}
              >
                Buscar citas disponibles
              </button>
            </form>
          </div>

          <div style={{ marginBottom: '1rem' }}>
            <p style={{ color: '#ddd', margin: 0 }}>{mensajeCitas}</p>
          </div>

          {citaEspecialidadActual && (
            <div style={{ marginBottom: '1rem', backgroundColor: 'rgba(255,193,7,0.18)', padding: '0.8rem', borderRadius: '8px', border: '1px solid #ffb300' }}>
              <p style={{ margin: 0, color: '#fff' }}><strong>Tienes una cita PENDIENTE en {citaEspecialidadActual.especialidad}:</strong></p>
              <p style={{ margin: '0.1rem 0 0', color: '#fff' }}>
                Médico: {citaEspecialidadActual.medicoNombre || 'N/D'}, fecha: {new Date(citaEspecialidadActual.fechaCita).toLocaleDateString()}, hora: {(citaEspecialidadActual.horaCita || '').slice(0, 5)}.
              </p>
              <p style={{ margin: '0.1rem 0 0', color: '#fff' }}>Al seleccionar otro horario disponible se reprogramará esta cita.</p>
            </div>
          )}

          {citasDisponibles.length > 0 && (
            <div style={{ marginBottom: '2rem' }}>
              <h3>Citas disponibles</h3>
              <div style={{ overflowX: 'auto' }}>
                <table style={{ width: '100%', borderCollapse: 'collapse', color: '#fff' }}>
                  <thead>
                    <tr style={{ borderBottom: '1px solid rgba(255,255,255,0.35)' }}>
                      <th style={{ textAlign: 'left', padding: '0.6rem' }}>Fecha</th>
                      <th style={{ textAlign: 'left', padding: '0.6rem' }}>Hora</th>
                      <th style={{ textAlign: 'left', padding: '0.6rem' }}>Médico</th>
                      <th style={{ textAlign: 'left', padding: '0.6rem' }}>Especialidad</th>
                      <th style={{ textAlign: 'left', padding: '0.6rem' }}>Acción</th>
                    </tr>
                  </thead>
                  <tbody>
                    {citasDisponibles.slice((paginaActual - 1) * citasPorPagina, paginaActual * citasPorPagina).map((disp, index) => (
                      <tr key={index} style={{ borderBottom: '1px solid rgba(255,255,255,0.2)' }}>
                        <td style={{ padding: '0.6rem' }}>{new Date(disp.fecha).toLocaleDateString()}</td>
                        <td style={{ padding: '0.6rem' }}>{disp.hora}</td>
                        <td style={{ padding: '0.6rem' }}>{disp.medicoNombre}</td>
                        <td style={{ padding: '0.6rem' }}>{disp.especialidad}</td>
                        <td style={{ padding: '0.6rem' }}>
                          <button
                            type="button"
                            onClick={() => seleccionarCita(disp)}
                            style={{
                              padding: '0.4rem 0.7rem',
                              borderRadius: '8px',
                              border: 'none',
                              backgroundColor: citaEspecialidadActual ? '#ff9800' : '#4caf50',
                              color: '#fff',
                              cursor: 'pointer',
                            }}
                          >
                            {citaEspecialidadActual ? 'Reprogramar cita' : 'Seleccionar cita'}
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              <div style={{ marginTop: '1rem', display: 'flex', justifyContent: 'center', gap: '0.5rem' }}>
                <button
                  onClick={() => setPaginaActual(Math.max(1, paginaActual - 1))}
                  disabled={paginaActual === 1}
                  style={{ padding: '0.5rem', borderRadius: '8px', border: 'none', backgroundColor: '#555', color: '#fff', cursor: 'pointer' }}
                >
                  Anterior
                </button>
                <span style={{ color: '#ddd' }}>Página {paginaActual} de {Math.ceil(citasDisponibles.length / citasPorPagina)}</span>
                <button
                  onClick={() => setPaginaActual(Math.min(Math.ceil(citasDisponibles.length / citasPorPagina), paginaActual + 1))}
                  disabled={paginaActual === Math.ceil(citasDisponibles.length / citasPorPagina)}
                  style={{ padding: '0.5rem', borderRadius: '8px', border: 'none', backgroundColor: '#555', color: '#fff', cursor: 'pointer' }}
                >
                  Siguiente
                </button>
              </div>
            </div>
          )}

          <h3 style={{ marginTop: '2rem', borderTop: '1px solid rgba(255,255,255,0.2)', paddingTop: '1.5rem' }}>Mis Citas</h3>
          <div style={{ display: 'flex', gap: '1rem', marginBottom: '1rem', flexWrap: 'wrap', alignItems: 'flex-end' }}>
            <div style={{ flex: '1 1 200px', minWidth: '150px' }}>
              <label style={{ display: 'block', marginBottom: '0.4rem' }}>Filtrar por Estado</label>
              <select
                value={filtroEstadoCita}
                onChange={(e) => setFiltroEstadoCita(e.target.value)}
                style={{ width: '100%', padding: '0.6rem', borderRadius: '8px', border: '1px solid #999', boxSizing: 'border-box' }}
              >
                <option value="">Todas</option>
                <option value="Pendiente">Pendiente</option>
                <option value="Confirmada">Confirmada</option>
                <option value="Cancelada">Cancelada</option>
              </select>
            </div>
            <div style={{ flex: '1 1 200px', minWidth: '150px' }}>
              <label style={{ display: 'block', marginBottom: '0.4rem' }}>Filtrar por Fecha</label>
              <input
                type="date"
                value={filtroFechaCita}
                onChange={(e) => setFiltroFechaCita(e.target.value)}
                style={{ width: '100%', padding: '0.6rem', borderRadius: '8px', border: '1px solid #999', boxSizing: 'border-box' }}
              />
            </div>
            <div style={{ flex: '1 1 200px', minWidth: '150px' }}>
              <label style={{ display: 'block', marginBottom: '0.4rem' }}>Filtrar por Especialidad</label>
              <select
                value={filtroEspecialidadCita}
                onChange={(e) => setFiltroEspecialidadCita(e.target.value)}
                style={{ width: '100%', padding: '0.6rem', borderRadius: '8px', border: '1px solid #999', boxSizing: 'border-box' }}
              >
                <option value="">Todas</option>
                {especialidades.map((esp) => (
                  <option key={esp.id} value={esp.nombre}>{esp.nombre}</option>
                ))}
              </select>
            </div>
            <button
              type="button"
              onClick={() => {
                setFiltroEstadoCita('Pendiente');
                setFiltroFechaCita('');
                setFiltroEspecialidadCita('');
              }}
              style={{ padding: '0.6rem 1rem', borderRadius: '8px', border: '1px solid #999', backgroundColor: '#777', color: '#fff', cursor: 'pointer', alignSelf: 'flex-end', boxSizing: 'border-box', flex: '0 0 auto' }}
            >
              Limpiar
            </button>
          </div>

          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', color: '#fff' }}>
              <thead>
                <tr style={{ borderBottom: '1px solid rgba(255,255,255,0.35)' }}>
                  <th style={{ textAlign: 'left', padding: '0.6rem' }}>Médico</th>
                  <th style={{ textAlign: 'left', padding: '0.6rem' }}>Especialidad</th>
                  <th style={{ textAlign: 'left', padding: '0.6rem' }}>Paciente</th>
                  <th style={{ textAlign: 'left', padding: '0.6rem' }}>Fecha</th>
                  <th style={{ textAlign: 'left', padding: '0.6rem' }}>Hora</th>
                  <th style={{ textAlign: 'left', padding: '0.6rem' }}>Estado</th>
                  <th style={{ textAlign: 'left', padding: '0.6rem' }}>Acción</th>
                </tr>
              </thead>
              <tbody>
                {citasFiltradas.map((cita) => (
                  <tr key={cita.id} style={{ borderBottom: '1px solid rgba(255,255,255,0.2)' }}>
                    <td style={{ padding: '0.6rem' }}>{cita.medicoNombre}</td>
                    <td style={{ padding: '0.6rem' }}>{cita.especialidad}</td>
                    <td style={{ padding: '0.6rem' }}>{cita.pacienteNombre}</td>
                    <td style={{ padding: '0.6rem' }}>{new Date(cita.fechaCita).toLocaleDateString()}</td>
                    <td style={{ padding: '0.6rem' }}>{(cita.horaCita || '').toString().slice(0, 5)}</td>
                    <td style={{ padding: '0.6rem' }}>{cita.estado}</td>
                    <td style={{ padding: '0.6rem', display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
                      <button
                        type="button"
                        onClick={() => descargarPdfCita(cita.id)}
                        style={{
                          padding: '0.4rem 0.7rem',
                          borderRadius: '8px',
                          border: 'none',
                          backgroundColor: '#2196f3',
                          color: '#fff',
                          cursor: 'pointer',
                        }}
                      >
                        Descargar PDF
                      </button>
                      {cita.estado !== 'Cancelada' && (
                        <button
                          type="button"
                          onClick={() => cancelarCita(cita.id)}
                          style={{
                            padding: '0.4rem 0.7rem',
                            borderRadius: '8px',
                            border: 'none',
                            backgroundColor: '#f44336',
                            color: '#fff',
                            cursor: 'pointer',
                          }}
                        >
                          Cancelar
                        </button>
                      )}
                    </td>
                  </tr>
                ))}

                {citasFiltradas.length === 0 && (
                  <tr>
                    <td colSpan={7} style={{ padding: '0.8rem', color: '#aaa' }}>
                      No hay citas cargadas
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      );
    }

    if (pestañaActiva === 'Ajustes') {
      if (!esMedico) {
        return <p style={{ color: '#ddd' }}>No tienes acceso a esta sección. Solo los médicos registrados pueden acceder a Ajustes.</p>;
      }

      return (
        <div>
          {medicoInfo && (
            <div style={{ marginBottom: '1.5rem', backgroundColor: 'rgba(76,175,80,0.15)', padding: '1rem', borderRadius: '8px', border: '1px solid #4caf50' }}>
              <h3 style={{ margin: '0 0 0.5rem 0', color: '#4caf50' }}>Información del Médico</h3>
              <p style={{ margin: '0.3rem 0', color: '#fff' }}><strong>Nombre:</strong> {medicoInfo.nombre} {medicoInfo.apellido}</p>
              <p style={{ margin: '0.3rem 0', color: '#fff' }}><strong>Especialidad:</strong> {medicoInfo.especialidad}</p>
              <p style={{ margin: '0.3rem 0', color: '#fff' }}><strong>Email:</strong> {medicoInfo.email}</p>
              <p style={{ margin: '0.3rem 0', color: '#fff' }}><strong>Teléfono:</strong> {medicoInfo.telefono}</p>
            </div>
          )}

          {mensajeAjustes && <p style={{ color: '#ffc107', marginBottom: '1rem' }}>{mensajeAjustes}</p>}

          <div style={{ marginBottom: '2rem', border: '1px solid rgba(255,255,255,0.25)', borderRadius: '12px', padding: '1.5rem', backgroundColor: 'rgba(0,0,0,0.2)' }}>
            <h3>Registrar Disponibilidad</h3>

            <form onSubmit={crearDisponibilidad}>
              <div style={{ marginBottom: '1rem', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '1rem' }}>
                <div>
                  <label style={{ display: 'block', marginBottom: '0.4rem' }}>Fecha de Disponibilidad</label>
                  <input
                    type="date"
                    value={fechaDisponibilidad}
                    onChange={(e) => setFechaDisponibilidad(e.target.value)}
                    style={{ width: '100%', padding: '0.6rem', borderRadius: '8px', border: '1px solid #999' }}
                    required
                  />
                </div>

                <div>
                  <label style={{ display: 'block', marginBottom: '0.4rem' }}>Hora de Inicio</label>
                  <input
                    type="time"
                    value={horaInicio}
                    onChange={(e) => setHoraInicio(e.target.value)}
                    style={{ width: '100%', padding: '0.6rem', borderRadius: '8px', border: '1px solid #999' }}
                  />
                </div>

                <div>
                  <label style={{ display: 'block', marginBottom: '0.4rem' }}>Hora de Fin</label>
                  <input
                    type="time"
                    value={horaFin}
                    onChange={(e) => setHoraFin(e.target.value)}
                    style={{ width: '100%', padding: '0.6rem', borderRadius: '8px', border: '1px solid #999' }}
                  />
                </div>

                <div>
                  <label style={{ display: 'block', marginBottom: '0.4rem' }}>Duración de Cita (minutos)</label>
                  <input
                    type="number"
                    value={duracionMinutos}
                    onChange={(e) => setDuracionMinutos(e.target.value)}
                    min="15"
                    max="120"
                    style={{ width: '100%', padding: '0.6rem', borderRadius: '8px', border: '1px solid #999' }}
                  />
                </div>
              </div>

              <button
                type="submit"
                disabled={cargandoAjustes}
                style={{
                  padding: '0.85rem 1.4rem',
                  borderRadius: '10px',
                  border: 'none',
                  backgroundColor: '#4caf50',
                  color: '#fff',
                  fontWeight: 700,
                  cursor: 'pointer',
                }}
              >
                {cargandoAjustes ? 'Creando...' : 'Crear Disponibilidad'}
              </button>
            </form>
          </div>

          <div style={{ marginBottom: '2rem' }}>
            <h3>Disponibilidades Registradas</h3>
            {disponibilidades.length === 0 ? (
              <p style={{ color: '#ddd' }}>No hay disponibilidades registradas aún.</p>
            ) : (
              <div style={{ overflowX: 'auto' }}>
                <table style={{ width: '100%', borderCollapse: 'collapse', color: '#fff' }}>
                  <thead>
                    <tr style={{ borderBottom: '1px solid rgba(255,255,255,0.35)' }}>
                      <th style={{ textAlign: 'left', padding: '0.6rem' }}>Fecha</th>
                      <th style={{ textAlign: 'left', padding: '0.6rem' }}>Hora Inicio</th>
                      <th style={{ textAlign: 'left', padding: '0.6rem' }}>Hora Fin</th>
                      <th style={{ textAlign: 'left', padding: '0.6rem' }}>Duración (min)</th>
                      <th style={{ textAlign: 'left', padding: '0.6rem' }}>Acción</th>
                    </tr>
                  </thead>
                  <tbody>
                    {disponibilidades.map((disp) => (
                      <tr key={disp.id} style={{ borderBottom: '1px solid rgba(255,255,255,0.2)' }}>
                        <td style={{ padding: '0.6rem' }}>{(disp.fechaDisponibilidad || '').slice(0, 10)}</td>
                        <td style={{ padding: '0.6rem' }}>{disp.horaInicio}</td>
                        <td style={{ padding: '0.6rem' }}>{disp.horaFin}</td>
                        <td style={{ padding: '0.6rem' }}>{disp.duracionCitaMinutos}</td>
                        <td style={{ padding: '0.6rem' }}>
                          <button
                            type="button"
                            onClick={() => eliminarDisponibilidad(disp.id)}
                            style={{
                              padding: '0.4rem 0.7rem',
                              borderRadius: '8px',
                              border: 'none',
                              backgroundColor: '#f44336',
                              color: '#fff',
                              cursor: 'pointer',
                            }}
                          >
                            Eliminar
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>

          <div style={{ marginBottom: '2rem', border: '1px solid rgba(255,255,255,0.25)', borderRadius: '12px', padding: '1.5rem', backgroundColor: 'rgba(0,0,0,0.2)' }}>
            <h3>Reporte de Citas</h3>
            <p style={{ color: '#ddd' }}>Total de citas: <strong>{citasMedicoReporte.length}</strong></p>
            {mensajeReporte && <p style={{ color: '#ffc107' }}>{mensajeReporte}</p>}

            <div style={{ marginBottom: '1rem', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '1rem' }}>
              <div>
                <label style={{ display: 'block', marginBottom: '0.4rem' }}>Fecha Inicio</label>
                <input
                  type="date"
                  value={filtroFechaInicio}
                  onChange={(e) => setFiltroFechaInicio(e.target.value)}
                  style={{ width: '100%', padding: '0.6rem', borderRadius: '8px', border: '1px solid #999' }}
                />
              </div>
              <div>
                <label style={{ display: 'block', marginBottom: '0.4rem' }}>Fecha Fin</label>
                <input
                  type="date"
                  value={filtroFechaFin}
                  onChange={(e) => setFiltroFechaFin(e.target.value)}
                  style={{ width: '100%', padding: '0.6rem', borderRadius: '8px', border: '1px solid #999' }}
                />
              </div>
              <div>
                <label style={{ display: 'block', marginBottom: '0.4rem' }}>Paciente</label>
                <input
                  type="text"
                  value={filtroPaciente}
                  onChange={(e) => setFiltroPaciente(e.target.value)}
                  placeholder="Nombre del paciente"
                  style={{ width: '100%', padding: '0.6rem', borderRadius: '8px', border: '1px solid #999' }}
                />
              </div>
              <div>
                <label style={{ display: 'block', marginBottom: '0.4rem' }}>Estado</label>
                <select
                  value={filtroEstado}
                  onChange={(e) => setFiltroEstado(e.target.value)}
                  style={{ width: '100%', padding: '0.6rem', borderRadius: '8px', border: '1px solid #999' }}
                >
                  <option value="">Todos</option>
                  <option value="Pendiente">Pendiente</option>
                  <option value="Confirmada">Confirmada</option>
                  <option value="Cancelada">Cancelada</option>
                </select>
              </div>
            </div>

            <div style={{ marginBottom: '1rem', display: 'flex', gap: '1rem' }}>
              <button
                type="button"
                onClick={cargarReporteCitasMedico}
                style={{
                  padding: '0.6rem 1.2rem',
                  borderRadius: '8px',
                  border: 'none',
                  backgroundColor: '#2196f3',
                  color: '#fff',
                  cursor: 'pointer',
                }}
              >
                Aplicar Filtros
              </button>

              <button
                type="button"
                onClick={descargarReportePdfMedico}
                style={{
                  padding: '0.6rem 1.2rem',
                  borderRadius: '8px',
                  border: 'none',
                  backgroundColor: '#4caf50',
                  color: '#fff',
                  cursor: 'pointer',
                }}
              >
                Descargar Reporte en PDF
              </button>
            </div>

            {citasMedicoReporte.length === 0 ? (
              <p style={{ color: '#ddd' }}>No hay citas registradas.</p>
            ) : (
              <div style={{ overflowX: 'auto' }}>
                <table style={{ width: '100%', borderCollapse: 'collapse', color: '#fff' }}>
                  <thead>
                    <tr style={{ borderBottom: '1px solid rgba(255,255,255,0.35)' }}>
                      <th style={{ textAlign: 'left', padding: '0.6rem' }}>Paciente</th>
                      <th style={{ textAlign: 'left', padding: '0.6rem' }}>Email Paciente</th>
                      <th style={{ textAlign: 'left', padding: '0.6rem' }}>Fecha</th>
                      <th style={{ textAlign: 'left', padding: '0.6rem' }}>Hora</th>
                      <th style={{ textAlign: 'left', padding: '0.6rem' }}>Estado</th>
                    </tr>
                  </thead>
                  <tbody>
                    {citasMedicoReporte.map((cita) => (
                      <tr key={cita.id} style={{ borderBottom: '1px solid rgba(255,255,255,0.2)' }}>
                        <td style={{ padding: '0.6rem' }}>{cita.pacienteNombre}</td>
                        <td style={{ padding: '0.6rem' }}>{cita.pacienteEmail || '-'}</td>
                        <td style={{ padding: '0.6rem' }}>{new Date(cita.fechaCita).toLocaleDateString()}</td>
                        <td style={{ padding: '0.6rem' }}>{cita.horaCita && cita.horaCita.toString().slice(0, 5)}</td>
                        <td style={{ padding: '0.6rem' }}>
                          <span style={{
                            padding: '0.3rem 0.6rem',
                            borderRadius: '4px',
                            backgroundColor: cita.estado === 'Pendiente' ? '#ff9800' : cita.estado === 'Cancelada' ? '#f44336' : '#4caf50',
                            color: '#fff',
                            fontSize: '0.85rem',
                          }}>
                            {cita.estado}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      );
    }

    if (pestañaActiva === 'Cerrar sesión') {
      return (
        <div>
          <p style={{ color: '#ddd' }}>Presiona el botón para cerrar sesión de tu cuenta.</p>
          <button
            type="button"
            onClick={() => {
              localStorage.removeItem('token');
              navigate('/login');
            }}
            style={{
              padding: '0.85rem 1.4rem',
              borderRadius: '10px',
              border: 'none',
              backgroundColor: '#f44336',
              color: '#fff',
              fontWeight: 700,
              cursor: 'pointer',
            }}
          >
            Cerrar sesión
          </button>
        </div>
      );
    }

    return <p style={{ color: '#ddd' }}>Selecciona una pestaña para ver contenido.</p>;
  };

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
            Selecciona una opción en las pestañas. El perfil se carga y edita desde aquí.
          </p>
        </header>

        <nav style={{ display: 'flex', gap: '0.75rem', flexWrap: 'wrap', marginBottom: '2rem' }}>
          {pestañas.map((tab) => (
            <button
              key={tab}
              type="button"
              onClick={() => setPestañaActiva(tab)}
              style={{
                flex: 1,
                minWidth: '140px',
                padding: '1rem 1.25rem',
                borderRadius: '12px',
                border: pestañaActiva === tab ? '2px solid #fff' : '1px solid rgba(255,255,255,0.35)',
                background: pestañaActiva === tab ? '#fff' : 'rgba(255,255,255,0.08)',
                color: pestañaActiva === tab ? '#000' : '#fff',
                cursor: 'pointer',
                fontWeight: 600,
              }}
            >
              {tab}
            </button>
          ))}
        </nav>

        <section style={{ padding: '1.5rem', background: 'rgba(255,255,255,0.08)', borderRadius: '16px' }}>
          <h2 style={{ marginTop: 0 }}>{pestañaActiva}</h2>
          {pestañaContenido()}
        </section>

        <footer style={{ marginTop: '2rem', color: '#bbb' }}>
          <p>¿Necesitas cambiar de cuenta? <Link to="/login" style={{ color: '#fff', textDecoration: 'underline' }}>Ir a inicio de sesión</Link></p>
        </footer>
      </div>
    </div>
  );
};

export default Menu;
