import { useState, useEffect } from 'react';

const Appointments = () => {
  const [appointments, setAppointments] = useState([]);
  const [title, setTitle] = useState('');
  const [date, setDate] = useState('');

  useEffect(() => {
    const fetchAppointments = async () => {
      try {
        const response = await fetch('http://localhost:5001appointments');
        if (response.ok) {
          const data = await response.json();
          setAppointments(data);
        }
      } catch (error) {
        console.error('Error fetching appointments:', error);
      }
    };

    fetchAppointments();
  }, []);

  const handleCreateAppointment = async (e) => {
    e.preventDefault();
    try {
      const response = await fetch('http://localhost:5001/appointments', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ title, date, userId: '00000000-0000-0000-0000-000000000000' }), // Hardcoded for now
      });

      if (response.ok) {
        const newAppointment = await response.json();
        setAppointments([...appointments, newAppointment]);
        setTitle('');
        setDate('');
      } else {
        alert('Failed to create appointment');
      }
    } catch (error) {
      console.error('Error creating appointment:', error);
      alert('An error occurred during appointment creation.');
    }
  };

  return (
    <div>
      <h2>Appointments</h2>
      <form onSubmit={handleCreateAppointment}>
        <h3>Create Appointment</h3>
        <div>
          <label>Title</label>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
          />
        </div>
        <div>
          <label>Date</label>
          <input
            type="datetime-local"
            value={date}
            onChange={(e) => setDate(e.target.value)}
          />
        </div>
        <button type="submit">Create</button>
      </form>
      <ul>
        {appointments.map((appointment) => (
          <li key={appointment.id}>
            {appointment.title} - {new Date(appointment.date).toLocaleString()}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default Appointments;
