import { useState } from 'react';
import axios from 'axios';
import toast from 'react-hot-toast'; // Toast import kiya
import Galaxy from './Galaxy';
import { useAuth } from '../context/AuthContext';

export default function TicketForm() {
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    category: 'General',
  });
  const [audioFile, setAudioFile] = useState(null);
  const [imageFile, setImageFile] = useState(null);
  const [loading, setLoading] = useState(false); // Naya loading state

  // IMPORTANT: Testing ke liye apna Customer Token
  //const customerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJyYWh1bEB0ZXN0LmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkN1c3RvbWVyIiwiZXhwIjoxNzkxNDg0MzQ0LCJpc3MiOiJTdXBwb3J0UHVsc2VBUEkiLCJhdWQiOiJTdXBwb3J0UHVsc2VDbGllbnQifQ.DVgDxrdoHa3ePRIcuajEDjNjX3VwZsNS0Armlcg2oeM";
  const { user } = useAuth();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true); // Loading shuru
    
    const data = new FormData();
    data.append('Title', formData.title);
    data.append('Description', formData.description);
    data.append('Category', formData.category);
    if (audioFile) data.append('AudioFile', audioFile);
    if (imageFile) data.append('ImageFile', imageFile);

    try {
      const response = await axios.post('http://localhost:5215/api/v1/Tickets', data, {
        headers: {
          'Content-Type': 'multipart/form-data',
          'Authorization': `Bearer ${user.token}`
        }
      });
      
      // Stylish Success Toast
      toast.success(`Ticket submitted successfully! ID: ${response.data.ticketId}`);
      
      // Form ko wapas empty karna
      setFormData({ title: '', description: '', category: 'General' });
      setAudioFile(null);
      setImageFile(null);
      e.target.reset(); // File inputs ko DOM se clear karne ke liye
      
    } catch (error) {
      // Stylish Error Toast
      const errorMsg = error.response?.data?.message || error.message;
      toast.error(`Error: ${errorMsg}`);
    } finally {
      setLoading(false); // Loading band
    }
  };

  return (
    <div className="relative h-[calc(100vh-4rem)] overflow-y-auto overflow-x-hidden bg-slate-100">
      <Galaxy
        className="opacity-70"
        density={1.1}
        glowIntensity={0.42}
        saturation={0.7}
        hueShift={205}
        rotationSpeed={0.06}
      />
      <div className="relative z-10 max-w-2xl mx-auto p-8 bg-white/90 backdrop-blur-sm shadow-lg border border-gray-100 rounded-xl mt-10">
      <h2 className="text-2xl font-bold text-gray-800 mb-6">Submit a Support Ticket</h2>
      
      <form onSubmit={handleSubmit} className="space-y-5">
        <div>
          <label className="block font-medium text-gray-700 mb-1">Title <span className="text-red-500">*</span></label>
          <input 
            type="text" 
            required 
            value={formData.title}
            className="w-full border border-gray-300 p-2.5 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all" 
            placeholder="Briefly describe your issue"
            onChange={(e) => setFormData({...formData, title: e.target.value})} 
          />
        </div>
        
        <div>
          <label className="block font-medium text-gray-700 mb-1">Description <span className="text-red-500">*</span></label>
          <textarea 
            required 
            value={formData.description}
            className="w-full border border-gray-300 p-2.5 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all" 
            rows="4"
            placeholder="Please provide all the necessary details..."
            onChange={(e) => setFormData({...formData, description: e.target.value})}
          ></textarea>
        </div>

        {/* Category Dropdown (Aapke state me tha, par UI me nahi tha) */}
        <div>
          <label className="block font-medium text-gray-700 mb-1">Category</label>
          <select 
            value={formData.category}
            onChange={(e) => setFormData({...formData, category: e.target.value})}
            className="w-full border border-gray-300 p-2.5 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all bg-white"
          >
            <option value="General">General</option>
            <option value="Technical">Technical Issue</option>
            <option value="Billing">Billing & Payments</option>
          </select>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block font-medium text-gray-700 mb-1">Screenshot (Optional)</label>
            <input 
              type="file" 
              accept="image/*" 
              className="w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100 cursor-pointer border border-gray-200 rounded-lg p-1"
              onChange={(e) => setImageFile(e.target.files[0])} 
            />
          </div>

          <div>
            <label className="block font-medium text-gray-700 mb-1">Voice Note (Optional)</label>
            <input 
              type="file" 
              accept="audio/*" 
              className="w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-sm file:font-semibold file:bg-green-50 file:text-green-700 hover:file:bg-green-100 cursor-pointer border border-gray-200 rounded-lg p-1"
              onChange={(e) => setAudioFile(e.target.files[0])} 
            />
          </div>
        </div>

        <button 
          type="submit" 
          disabled={loading}
          className="w-full mt-4 bg-blue-600 text-white font-semibold p-3 rounded-lg hover:bg-blue-700 transition-colors disabled:bg-blue-400 flex justify-center items-center gap-2"
        >
          {loading ? (
            <>
              <svg className="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              Submitting...
            </>
          ) : (
            'Submit Ticket'
          )}
        </button>
      </form>
      </div>
    </div>
  );
}