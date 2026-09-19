import { useState } from 'react';
import axios from 'axios';
import toast from 'react-hot-toast';
import Galaxy from './Galaxy';
import { useAuth } from '../context/AuthContext';

export default function TicketForm() {
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    category: 'Low', 
  });
  const [audioFile, setAudioFile] = useState(null);
  const [imageFile, setImageFile] = useState(null);
  const [loading, setLoading] = useState(false);

  const { user } = useAuth();

  // 🎨 Dropdown ka color change karne wala function
  const getCategoryColor = (cat) => {
    if (cat === 'High') return 'bg-red-50 text-red-700 border-red-300';
    if (cat === 'Medium') return 'bg-yellow-50 text-yellow-700 border-yellow-300';
    if (cat === 'Low') return 'bg-green-50 text-green-700 border-green-300';
    return 'bg-white text-gray-700 border-gray-300'; // Default
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true); 
    
    const data = new FormData();
    data.append('Title', formData.title);
    data.append('Description', formData.description);
    data.append('Category', formData.category);
    if (audioFile) data.append('AudioFile', audioFile);
    if (imageFile) data.append('ImageFile', imageFile);

    try {
        const response = await axios.post('http://34.93.237.221:5215/api/v1/Tickets', data, {
        // const response = await axios.post('http://localhost:5215/api/v1/Tickets', data, {
        headers: {
          'Content-Type': 'multipart/form-data',
          'Authorization': `Bearer ${user.token}`
        }
      });
      
      toast.success(`Ticket submitted successfully! ID: ${response.data.ticketId}`);
      
      setFormData({ title: '', description: '', category: 'Low' });
      setAudioFile(null);
      setImageFile(null);
      e.target.reset(); 
      
    } catch (error) {
      const errorMsg = error.response?.data?.message || error.message;
      toast.error(`Error: ${errorMsg}`);
    } finally {
      setLoading(false); 
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

        {/* 🔥 UPDATED: Category Dropdown with Colors and correct values */}
        <div>
          <label className="block font-medium text-gray-700 mb-1">Priority (Category)</label>
          <select 
            value={formData.category}
            onChange={(e) => setFormData({...formData, category: e.target.value})}
            className={`w-full border p-2.5 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all font-semibold ${getCategoryColor(formData.category)}`}
          >
            <option value="Low" className="text-green-700 bg-white font-semibold">Low</option>
            <option value="Medium" className="text-yellow-700 bg-white font-semibold">Medium</option>
            <option value="High" className="text-red-700 bg-white font-semibold">High</option>
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