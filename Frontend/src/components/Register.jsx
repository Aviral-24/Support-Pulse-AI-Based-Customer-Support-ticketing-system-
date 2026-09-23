// import { useState } from 'react';
// import axios from 'axios';
// import { useNavigate, Link } from 'react-router-dom';
// import toast from 'react-hot-toast';
// import Galaxy from './Galaxy';

// export default function Register() {
//   const [name, setName] = useState('');
//   const [email, setEmail] = useState('');
//   const [password, setPassword] = useState('');
//   const [loading, setLoading] = useState(false);
//   const navigate = useNavigate();

//   const handleSubmit = async (e) => {
//     e.preventDefault();
//     setLoading(true);
//     try {
//       await axios.post('http://localhost:5215/api/v1/Auth/register', { name, email, password });
//       toast.success('Registration successful! Please login.');
//       navigate('/login');
//     } catch (error) {
//       toast.error(error.response?.data || 'Registration failed.');
//     } finally {
//       setLoading(false);
//     }
//   };

//   return (
//     <div className="relative h-[calc(100vh-4rem)] flex items-center justify-center bg-slate-100 overflow-hidden">
//       <Galaxy className="opacity-70" density={1.1} glowIntensity={0.4} saturation={0.6} />
//       <div className="relative z-10 w-full max-w-md p-8 bg-white/90 backdrop-blur-md rounded-2xl shadow-xl border border-gray-200">
//         <h2 className="text-3xl font-bold text-center text-gray-800 mb-6">Create Account</h2>
//         <form onSubmit={handleSubmit} className="space-y-4">
//           <div>
//             <label className="block text-sm font-medium text-gray-700 mb-1">Full Name</label>
//             <input type="text" required value={name} onChange={(e) => setName(e.target.value)}
//               className="w-full border border-gray-300 p-3 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" placeholder="John Doe" />
//           </div>
//           <div>
//             <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
//             <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)}
//               className="w-full border border-gray-300 p-3 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" placeholder="you@example.com" />
//           </div>
//           <div>
//             <label className="block text-sm font-medium text-gray-700 mb-1">Password</label>
//             <input type="password" required value={password} onChange={(e) => setPassword(e.target.value)}
//               className="w-full border border-gray-300 p-3 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" placeholder="••••••••" />
//           </div>
//           <button type="submit" disabled={loading} className="w-full bg-blue-600 text-white font-bold py-3 rounded-lg hover:bg-blue-700 transition">
//             {loading ? 'Creating Account...' : 'Register'}
//           </button>
//         </form>
//         <p className="mt-4 text-center text-sm text-gray-600">
//           Already have an account? <Link to="/login" className="text-blue-600 font-semibold hover:underline">Login here</Link>
//         </p>
//       </div>
//     </div>
//   );
// }


// import { useState } from 'react';
// import axios from 'axios';
// import { useNavigate, Link } from 'react-router-dom';
// import toast from 'react-hot-toast';
// import Galaxy from './Galaxy';

// export default function Register() {
//   const [name, setName] = useState('');
//   const [email, setEmail] = useState('');
//   const [password, setPassword] = useState('');
//   const [role, setRole] = useState('Customer'); // 🟢 Naya state Role ke liye
//   const [loading, setLoading] = useState(false);
//   const navigate = useNavigate();

//   const handleSubmit = async (e) => {
//     e.preventDefault();
//     setLoading(true);
//     try {
//       // 🟢 API payload me role pass kiya gaya hai
//       await axios.post('http://localhost:5215/api/v1/Auth/register', { name, email, password, role });
//       toast.success('Registration successful! Please login.');
//       navigate('/login');
//     } catch (error) {
//       toast.error(error.response?.data || 'Registration failed.');
//     } finally {
//       setLoading(false);
//     }
//   };

//   return (
//     <div className="relative h-screen flex items-center justify-center bg-slate-100 overflow-hidden">
//       <Galaxy className="opacity-70" density={1.1} glowIntensity={0.4} saturation={0.6} />
//       <div className="relative z-10 w-full max-w-md p-8 bg-white/90 backdrop-blur-md rounded-2xl shadow-xl border border-gray-200">
//         <h2 className="text-3xl font-bold text-center text-gray-800 mb-6">Create Account</h2>
//         <form onSubmit={handleSubmit} className="space-y-4">
//           <div>
//             <label className="block text-sm font-medium text-gray-700 mb-1">Full Name</label>
//             <input type="text" required value={name} onChange={(e) => setName(e.target.value)}
//               className="w-full border border-gray-300 p-3 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" placeholder="John Doe" />
//           </div>
//           <div>
//             <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
//             <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)}
//               className="w-full border border-gray-300 p-3 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" placeholder="you@example.com" />
//           </div>
//           <div>
//             <label className="block text-sm font-medium text-gray-700 mb-1">Password</label>
//             <input type="password" required value={password} onChange={(e) => setPassword(e.target.value)}
//               className="w-full border border-gray-300 p-3 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" placeholder="••••••••" />
//           </div>
          
//           {/* 🟢 NAYA DROPDOWN UI ME ADD KIYA */}
//           <div>
//             <label className="block text-sm font-medium text-gray-700 mb-1">Select Role</label>
//             <select value={role} onChange={(e) => setRole(e.target.value)}
//               className="w-full border border-gray-300 p-3 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none bg-white">
//               <option value="Customer">Customer</option>
//               <option value="Agent">Support Agent</option>
//             </select>
//           </div>

//           <button type="submit" disabled={loading} className="w-full bg-blue-600 text-white font-bold py-3 rounded-lg hover:bg-blue-700 transition">
//             {loading ? 'Creating Account...' : 'Register'}
//           </button>
//         </form>
//         <p className="mt-4 text-center text-sm text-gray-600">
//           Already have an account? <Link to="/login" className="text-blue-600 font-semibold hover:underline">Login here</Link>
//         </p>
//       </div>
//     </div>
//   );
// }



// import { useState } from 'react';
// import axios from 'axios';
// import { useNavigate, Link } from 'react-router-dom';
// import toast from 'react-hot-toast';

// export default function Register() {
//   const [name, setName] = useState('');
//   const [email, setEmail] = useState('');
//   const [password, setPassword] = useState('');
//   const [role, setRole] = useState('Customer');
//   const [loading, setLoading] = useState(false);
//   const navigate = useNavigate();

//   const handleSubmit = async (e) => {
//     e.preventDefault();
//     setLoading(true);
//     try {
//       await axios.post('http://localhost:5215/api/v1/Auth/register', { name, email, password, role });
//       toast.success('Registration successful! Please login.');
//       navigate('/login');
//     } catch (error) {
//       toast.error(error.response?.data || 'Registration failed.');
//     } finally {
//       setLoading(false);
//     }
//   };

//   return (
//     // Dark Perforated Background
//     <div className="relative min-h-[calc(100vh-4rem)] flex items-center justify-center bg-[#0d0d0d] bg-[image:radial-gradient(#333_1px,transparent_1px)] bg-[size:12px_12px] overflow-hidden">
      
//       {/* Circular Glass Container (Slightly larger for more fields) */}
//       <div className="relative flex items-center justify-center w-[480px] h-[480px] rounded-full bg-gradient-to-br from-white/10 to-black/60 backdrop-blur-md border border-white/20 shadow-[0_20px_50px_rgba(0,0,0,0.8)]">
        
//         {/* Neon Green Glow Ring */}
//         <div className="absolute w-[430px] h-[430px] rounded-full border-[3px] border-[#65a30d]/50 shadow-[0_0_30px_rgba(101,163,13,0.4),inset_0_0_30px_rgba(101,163,13,0.4)] pointer-events-none"></div>

//         {/* Form Content */}
//         <div className="relative z-10 w-full max-w-[260px] flex flex-col items-center mt-2">
//           <form onSubmit={handleSubmit} className="w-full space-y-3">
            
//             <div className="w-full">
//               <input type="text" required value={name} onChange={(e) => setName(e.target.value)}
//                 className="w-full bg-white/90 text-black px-4 py-2 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner text-sm font-medium" 
//                 placeholder="Full Name" 
//               />
//             </div>

//             <div className="w-full">
//               <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)}
//                 className="w-full bg-white/90 text-black px-4 py-2 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner text-sm font-medium" 
//                 placeholder="Email Address" 
//               />
//             </div>

//             <div className="w-full">
//               <input type="password" required value={password} onChange={(e) => setPassword(e.target.value)}
//                 className="w-full bg-white/90 text-black px-4 py-2 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner text-sm font-medium" 
//                 placeholder="Password" 
//               />
//             </div>
            
//             <div className="w-full">
//               <select value={role} onChange={(e) => setRole(e.target.value)}
//                 className="w-full bg-white/90 text-black px-4 py-2 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner text-sm font-medium cursor-pointer">
//                 <option value="Customer">Customer</option>
//                 <option value="Agent">Support Agent</option>
//               </select>
//             </div>

//             <div className="w-full pt-2 text-center">
//               <button type="submit" disabled={loading} 
//                 className="text-[#84cc16] text-xl font-light tracking-widest hover:text-white hover:shadow-[0_0_15px_#84cc16] transition-all duration-300">
//                 {loading ? '...' : 'Register'}
//               </button>
//             </div>
//           </form>

//           <p className="mt-6 text-xs text-gray-400">
//             Have an account? <Link to="/login" className="text-white hover:text-[#84cc16] hover:underline transition-colors">Login</Link>
//           </p>
//         </div>
//       </div>
//     </div>
//   );
// }


// import { useState } from 'react';
// import axios from 'axios';
// import { useNavigate, Link } from 'react-router-dom';
// import toast from 'react-hot-toast';
// import Galaxy from './Galaxy'; // 🟢 Galaxy component wapas add kiya

// export default function Register() {
//   const [name, setName] = useState('');
//   const [email, setEmail] = useState('');
//   const [password, setPassword] = useState('');
//   const [role, setRole] = useState('Customer');
//   const [loading, setLoading] = useState(false);
//   const navigate = useNavigate();

//   const handleSubmit = async (e) => {
//     e.preventDefault();
//     setLoading(true);
//     try {
//       await axios.post('http://localhost:5215/api/v1/Auth/register', { name, email, password, role });
//       toast.success('Registration successful! Please login.');
//       navigate('/login');
//     } catch (error) {
//       toast.error(error.response?.data || 'Registration failed.');
//     } finally {
//       setLoading(false);
//     }
//   };

//   return (
//     <div className="relative min-h-[calc(100vh-4rem)] flex items-center justify-center bg-[#050505] overflow-hidden">
      
//       {/* 🟢 Galaxy Animation Background */}
//       <div className="absolute inset-0 z-0">
//         <Galaxy className="opacity-80" density={1.2} glowIntensity={0.5} saturation={0.7} />
//       </div>

//       {/* Circular Glass Container */}
//       <div className="relative z-10 flex items-center justify-center w-[480px] h-[480px] rounded-full bg-gradient-to-br from-white/10 to-black/60 backdrop-blur-md border border-white/20 shadow-[0_20px_50px_rgba(0,0,0,0.8)]">
        
//         {/* Neon Green Glow Ring */}
//         <div className="absolute w-[430px] h-[430px] rounded-full border-[3px] border-[#65a30d]/50 shadow-[0_0_30px_rgba(101,163,13,0.4),inset_0_0_30px_rgba(101,163,13,0.4)] pointer-events-none"></div>

//         {/* Form Content */}
//         <div className="relative z-20 w-full max-w-[260px] flex flex-col items-center mt-2">
//           <form onSubmit={handleSubmit} className="w-full space-y-3">
            
//             <div className="w-full">
//               <input type="text" required value={name} onChange={(e) => setName(e.target.value)}
//                 className="w-full bg-white/90 text-black px-4 py-2 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner text-sm font-medium" 
//                 placeholder="Full Name" 
//               />
//             </div>

//             <div className="w-full">
//               <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)}
//                 className="w-full bg-white/90 text-black px-4 py-2 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner text-sm font-medium" 
//                 placeholder="Email Address" 
//               />
//             </div>

//             <div className="w-full">
//               <input type="password" required value={password} onChange={(e) => setPassword(e.target.value)}
//                 className="w-full bg-white/90 text-black px-4 py-2 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner text-sm font-medium" 
//                 placeholder="Password" 
//               />
//             </div>
            
//             <div className="w-full">
//               <select value={role} onChange={(e) => setRole(e.target.value)}
//                 className="w-full bg-white/90 text-black px-4 py-2 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner text-sm font-medium cursor-pointer">
//                 <option value="Customer">Customer</option>
//                 <option value="Agent">Support Agent</option>
//               </select>
//             </div>

//             <div className="w-full pt-2 text-center">
//               <button type="submit" disabled={loading} 
//                 className="text-[#84cc16] text-xl font-light tracking-widest hover:text-white hover:shadow-[0_0_15px_#84cc16] transition-all duration-300">
//                 {loading ? '...' : 'REGISTER'}
//               </button>
//             </div>
//           </form>

//           <p className="mt-6 text-xs text-gray-400">
//             Have an account? <Link to="/login" className="text-white hover:text-[#84cc16] hover:underline transition-colors tracking-wide">Login</Link>
//           </p>
//         </div>
//       </div>
//     </div>
//   );
// }


// import { useState } from 'react';
// import axios from 'axios';
// import { useNavigate, Link } from 'react-router-dom';
// import toast from 'react-hot-toast';
// import Galaxy from './Galaxy';

// export default function Register() {
//   const [name, setName] = useState('');
//   const [email, setEmail] = useState('');
//   const [password, setPassword] = useState('');
//   const [role, setRole] = useState('Customer');
//   const [loading, setLoading] = useState(false);
//   const navigate = useNavigate();

//   const handleSubmit = async (e) => {
//     e.preventDefault();
//     setLoading(true);
//     try {
//       await axios.post('http://localhost:5215/api/v1/Auth/register', { name, email, password, role });
//            // await axios.post('http://34.93.237.221:5215/api/v1/Auth/register', { name, email, password, role });
//       toast.success('Registration successful! Please login.');
//       navigate('/login');
//     } catch (error) {
//       toast.error(error.response?.data || 'Registration failed.');
//     } finally {
//       setLoading(false);
//     }
//   };

//   return (
//     // 🟢 FIX: h-screen w-full kar diya
//     <div className="relative h-screen w-full flex items-center justify-center bg-[#050505] overflow-hidden">
      
//       {/* Galaxy Animation Background */}
//       <div className="absolute inset-0 z-0 w-full h-full">
//         <Galaxy className="opacity-80" density={1.2} glowIntensity={0.5} saturation={0.7} />
//       </div>

//       {/* Circular Glass Container (Bada size taaki 4 fields aaram se aayein) */}
//       <div className="relative z-10 flex items-center justify-center w-[520px] h-[520px] rounded-full bg-gradient-to-br from-white/10 to-black/60 backdrop-blur-md border border-white/20 shadow-[0_20px_50px_rgba(0,0,0,0.8)]">
        
//         {/* Neon Green Glow Ring */}
//         <div className="absolute w-[470px] h-[470px] rounded-full border-[3px] border-[#65a30d]/50 shadow-[0_0_40px_rgba(101,163,13,0.4),inset_0_0_40px_rgba(101,163,13,0.4)] pointer-events-none"></div>

//         {/* Form Content */}
//         <div className="relative z-20 w-full max-w-[320px] flex flex-col items-center mt-2">
//           <form onSubmit={handleSubmit} className="w-full space-y-4"> {/* 🟢 space-y-4 se fields ke beech gap */}
            
//             <div className="w-full">
//               <input type="text" required value={name} onChange={(e) => setName(e.target.value)}
//                 className="w-full bg-white/90 text-black px-5 py-2.5 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner text-sm font-medium transition-all" 
//                 placeholder="Full Name" 
//               />
//             </div>

//             <div className="w-full">
//               <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)}
//                 className="w-full bg-white/90 text-black px-5 py-2.5 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner text-sm font-medium transition-all" 
//                 placeholder="Email Address" 
//               />
//             </div>

//             <div className="w-full">
//               <input type="password" required value={password} onChange={(e) => setPassword(e.target.value)}
//                 className="w-full bg-white/90 text-black px-5 py-2.5 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner text-sm font-medium transition-all" 
//                 placeholder="Password" 
//               />
//             </div>
            
//             <div className="w-full">
//               <select value={role} onChange={(e) => setRole(e.target.value)}
//                 className="w-full bg-white/90 text-black px-5 py-2.5 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner text-sm font-medium cursor-pointer transition-all">
//                 <option value="Customer">Customer</option>
//                 <option value="Agent">Support Agent</option>
//               </select>
//             </div>

//             <div className="w-full pt-3 text-center">
//               <button type="submit" disabled={loading} 
//                 className="text-[#84cc16] text-xl font-light tracking-widest hover:text-white hover:shadow-[0_0_20px_#84cc16] transition-all duration-300">
//                 {loading ? '...' : 'REGISTER'}
//               </button>
//             </div>
//           </form>

//           <p className="mt-7 text-sm text-gray-400">
//             Have an account? <Link to="/login" className="text-white hover:text-[#84cc16] hover:underline transition-colors tracking-wide font-medium">Login</Link>
//           </p>
//         </div>
//       </div>
//     </div>
//   );
// }


import { useState } from 'react';
import axios from 'axios';
import { useNavigate, Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import Galaxy from './Galaxy';

export default function Register() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [role, setRole] = useState('Customer'); // Default role
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      // API call me role direct ja raha hai
         // await axios.post('http://localhost:5215/api/v1/Auth/register', { name, email, password, role });
         await axios.post('http://34.93.237.221:5215/api/v1/Auth/register', { name, email, password, role });
      toast.success('Registration successful! Please login.');
      navigate('/login');
    } catch (error) {
      toast.error(error.response?.data || 'Registration failed.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="relative h-screen w-full flex items-center justify-center bg-[#050505] overflow-hidden">
      
      {/* Galaxy Animation Background */}
      <div className="absolute inset-0 z-0 w-full h-full">
        <Galaxy className="opacity-80" density={1.2} glowIntensity={0.5} saturation={0.7} />
      </div>

      <div className="relative z-10 flex items-center justify-center w-[520px] h-[520px] rounded-full bg-gradient-to-br from-white/10 to-black/60 backdrop-blur-md border border-white/20 shadow-[0_20px_50px_rgba(0,0,0,0.8)]">
        
        <div className="absolute w-[470px] h-[470px] rounded-full border-[3px] border-[#65a30d]/50 shadow-[0_0_40px_rgba(101,163,13,0.4),inset_0_0_40px_rgba(101,163,13,0.4)] pointer-events-none"></div>

        <div className="relative z-20 w-full max-w-[320px] flex flex-col items-center mt-2">
          <form onSubmit={handleSubmit} className="w-full space-y-4">
            
            <div className="w-full">
              <input type="text" required value={name} onChange={(e) => setName(e.target.value)}
                className="w-full bg-white/90 text-black px-5 py-2.5 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner text-sm font-medium transition-all" 
                placeholder="Full Name" />
            </div>

            <div className="w-full">
              <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)}
                className="w-full bg-white/90 text-black px-5 py-2.5 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner text-sm font-medium transition-all" 
                placeholder="Email Address" />
            </div>

            <div className="w-full">
              <input type="password" required value={password} onChange={(e) => setPassword(e.target.value)}
                className="w-full bg-white/90 text-black px-5 py-2.5 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner text-sm font-medium transition-all" 
                placeholder="Password" />
            </div>
            
            {/*  Agent ko Admin bana diya */}
            <div className="w-full">
              <select value={role} onChange={(e) => setRole(e.target.value)}
                className="w-full bg-white/90 text-black px-5 py-2.5 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner text-sm font-medium cursor-pointer transition-all">
                <option value="Customer">User</option>
                <option value="Admin">Admin</option>
              </select>
            </div>

            <div className="w-full pt-3 text-center">
              <button type="submit" disabled={loading} 
                className="text-[#84cc16] text-xl font-light tracking-widest hover:text-white hover:shadow-[0_0_20px_#84cc16] transition-all duration-300">
                {loading ? '...' : 'REGISTER'}
              </button>
            </div>
          </form>

          <p className="mt-7 text-sm text-gray-400">
            Have an account? <Link to="/login" className="text-white hover:text-[#84cc16] hover:underline transition-colors tracking-wide font-medium">Login</Link>
          </p>
        </div>
      </div>
    </div>
  );
}