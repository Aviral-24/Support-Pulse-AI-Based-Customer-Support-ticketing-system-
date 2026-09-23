// import { useState } from 'react';
// import axios from 'axios';
// import { useNavigate, Link } from 'react-router-dom';
// import toast from 'react-hot-toast';
// import { useAuth } from '../context/AuthContext';
// import Galaxy from './Galaxy';

// export default function Login() {
//   const [email, setEmail] = useState('');
//   const [password, setPassword] = useState('');
//   const [loading, setLoading] = useState(false);
//   const navigate = useNavigate();
//   const { login } = useAuth();

//   const handleSubmit = async (e) => {
//     e.preventDefault();
//     setLoading(true);
//     try {
//       // Backend ko login request (Dhyan rakhein port 5215 hai)
//       const response = await axios.post('http://localhost:5215/api/v1/Auth/login', { email, password });
      
//       const { token, role } = response.data;
//       login(token, role); // Context me save kiya
      
//       toast.success('Logged in successfully!');
      
//       // Role ke hisaab se redirect
//       if (role === 'Agent' || role === 'Admin') {
//         navigate('/dashboard');
//       } else {
//         navigate('/'); // Customer ko ticket form par bhejenge
//       }
//     } catch (error) {
//       toast.error(error.response?.data || 'Login failed. Please check credentials.');
//     } finally {
//       setLoading(false);
//     }
//   };

//   return (
//     <div className="relative h-[calc(100vh-4rem)] flex items-center justify-center bg-slate-100 overflow-hidden">
//       <Galaxy className="opacity-70" density={1.1} glowIntensity={0.4} saturation={0.6} />
//       <div className="relative z-10 w-full max-w-md p-8 bg-white/90 backdrop-blur-md rounded-2xl shadow-xl border border-gray-200">
//         <h2 className="text-3xl font-bold text-center text-gray-800 mb-6">Welcome Back</h2>
//         <form onSubmit={handleSubmit} className="space-y-5">
//           <div>
//             <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
//             <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)}
//               className="w-full border border-gray-300 p-3 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" placeholder="agent@support.com" />
//           </div>
//           <div>
//             <label className="block text-sm font-medium text-gray-700 mb-1">Password</label>
//             <input type="password" required value={password} onChange={(e) => setPassword(e.target.value)}
//               className="w-full border border-gray-300 p-3 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" placeholder="••••••••" />
//           </div>
//           <button type="submit" disabled={loading} className="w-full bg-blue-600 text-white font-bold py-3 rounded-lg hover:bg-blue-700 transition">
//             {loading ? 'Authenticating...' : 'Login'}
//           </button>
//         </form>
//         <p className="mt-4 text-center text-sm text-gray-600">
//           Don't have an account? <Link to="/register" className="text-blue-600 font-semibold hover:underline">Register here</Link>
//         </p>
//       </div>
//     </div>
//   );
// }


// import { useState } from 'react';
// import axios from 'axios';
// import { useNavigate, Link } from 'react-router-dom';
// import toast from 'react-hot-toast';
// import { useAuth } from '../context/AuthContext'; // Aapka context import
// import Galaxy from './Galaxy';

// export default function Login() {
//   const [email, setEmail] = useState('');
//   const [password, setPassword] = useState('');
//   const [loading, setLoading] = useState(false);
//   const navigate = useNavigate();
//   const { login } = useAuth(); // Context se login method liya

//   const handleSubmit = async (e) => {
//     e.preventDefault();
//     setLoading(true);
//     try {
//       const response = await axios.post('http://localhost:5215/api/v1/Auth/login', { email, password });
      
//       const { token, role } = response.data; // Backend se token aur role nikaala
//       login(token, role); // Context me save kar liya
      
//       toast.success('Logged in successfully!');
      
//       // 🟢 SMART REDIRECT: Role ke basis par alag pages par bhejna
//       if (role === 'Agent' || role === 'Admin') {
//         navigate('/agent-dashboard');
//       } else {
//         navigate('/submit-ticket');
//       }
//     } catch (error) {
//       toast.error(error.response?.data?.message || error.response?.data || 'Login failed. Please check credentials.');
//     } finally {
//       setLoading(false);
//     }
//   };

//   return (
//     <div className="relative h-screen flex items-center justify-center bg-slate-100 overflow-hidden">
//       <Galaxy className="opacity-70" density={1.1} glowIntensity={0.4} saturation={0.6} />
//       <div className="relative z-10 w-full max-w-md p-8 bg-white/90 backdrop-blur-md rounded-2xl shadow-xl border border-gray-200">
//         <h2 className="text-3xl font-bold text-center text-gray-800 mb-6">Welcome Back</h2>
//         <form onSubmit={handleSubmit} className="space-y-5">
//           <div>
//             <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
//             <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)}
//               className="w-full border border-gray-300 p-3 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" placeholder="user@support.com" />
//           </div>
//           <div>
//             <label className="block text-sm font-medium text-gray-700 mb-1">Password</label>
//             <input type="password" required value={password} onChange={(e) => setPassword(e.target.value)}
//               className="w-full border border-gray-300 p-3 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" placeholder="••••••••" />
//           </div>
//           <button type="submit" disabled={loading} className="w-full bg-blue-600 text-white font-bold py-3 rounded-lg hover:bg-blue-700 transition">
//             {loading ? 'Authenticating...' : 'Login'}
//           </button>
//         </form>
//         <p className="mt-4 text-center text-sm text-gray-600">
//           Don't have an account? <Link to="/register" className="text-blue-600 font-semibold hover:underline">Register here</Link>
//         </p>
//       </div>
//     </div>
//   );
// }

// import { useState } from 'react';
// import axios from 'axios';
// import { useNavigate, Link } from 'react-router-dom';
// import toast from 'react-hot-toast';
// import { useAuth } from '../context/AuthContext';

// export default function Login() {
//   const [email, setEmail] = useState('');
//   const [password, setPassword] = useState('');
//   const [loading, setLoading] = useState(false);
//   const navigate = useNavigate();
//   const { login } = useAuth();

//   const handleSubmit = async (e) => {
//     e.preventDefault();
//     setLoading(true);
//     try {
//       const response = await axios.post('http://localhost:5215/api/v1/Auth/login', { email, password });
      
//       const { token, role } = response.data;
//       login(token, role);
//       toast.success('Logged in successfully!');
      
//       if (role === 'Agent' || role === 'Admin') {
//         navigate('/agent-dashboard');
//       } else {
//         navigate('/submit-ticket');
//       }
//     } catch (error) {
//       toast.error(error.response?.data?.message || error.response?.data || 'Login failed.');
//     } finally {
//       setLoading(false);
//     }
//   };

//   return (
//     // Dark Perforated Background
//     <div className="relative min-h-[calc(100vh-4rem)] flex items-center justify-center bg-[#0d0d0d] bg-[image:radial-gradient(#333_1px,transparent_1px)] bg-[size:12px_12px] overflow-hidden">
      
//       {/* Circular Glass Container */}
//       <div className="relative flex items-center justify-center w-[400px] h-[400px] rounded-full bg-gradient-to-br from-white/10 to-black/60 backdrop-blur-md border border-white/20 shadow-[0_20px_50px_rgba(0,0,0,0.8)]">
        
//         {/* Neon Green Glow Ring */}
//         <div className="absolute w-[350px] h-[350px] rounded-full border-[3px] border-[#65a30d]/50 shadow-[0_0_30px_rgba(101,163,13,0.4),inset_0_0_30px_rgba(101,163,13,0.4)] pointer-events-none"></div>

//         {/* Form Content */}
//         <div className="relative z-10 w-full max-w-[240px] flex flex-col items-center">
//           <form onSubmit={handleSubmit} className="w-full space-y-4">
            
//             <div className="w-full">
//               <label className="block text-xs text-gray-400 mb-1 ml-2">Username / Email</label>
//               <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)}
//                 className="w-full bg-white/90 text-black px-4 py-2.5 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner font-medium" 
//                 placeholder="agent@support.com" 
//               />
//             </div>

//             <div className="w-full">
//               <label className="block text-xs text-gray-400 mb-1 ml-2">Password</label>
//               <input type="password" required value={password} onChange={(e) => setPassword(e.target.value)}
//                 className="w-full bg-white/90 text-black px-4 py-2.5 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner font-medium" 
//                 placeholder="••••••••" 
//               />
//             </div>

//             <div className="w-full pt-4 text-center">
//               <button type="submit" disabled={loading} 
//                 className="text-[#84cc16] text-2xl font-light tracking-widest hover:text-white hover:shadow-[0_0_15px_#84cc16] transition-all duration-300">
//                 {loading ? '...' : 'Login'}
//               </button>
//             </div>
//           </form>

//           <p className="mt-8 text-xs text-gray-400">
//             No account? <Link to="/register" className="text-white hover:text-[#84cc16] hover:underline transition-colors">Register</Link>
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
// import { useAuth } from '../context/AuthContext';
// import Galaxy from './Galaxy'; // 🟢 Galaxy component wapas add kiya

// export default function Login() {
//   const [email, setEmail] = useState('');
//   const [password, setPassword] = useState('');
//   const [loading, setLoading] = useState(false);
//   const navigate = useNavigate();
//   const { login } = useAuth();

//   const handleSubmit = async (e) => {
//     e.preventDefault();
//     setLoading(true);
//     try {
//       const response = await axios.post('http://localhost:5215/api/v1/Auth/login', { email, password });
      
//       const { token, role } = response.data;
//       login(token, role);
//       toast.success('Logged in successfully!');
      
//       if (role === 'Agent' || role === 'Admin') {
//         navigate('/agent-dashboard');
//       } else {
//         navigate('/submit-ticket');
//       }
//     } catch (error) {
//       toast.error(error.response?.data?.message || error.response?.data || 'Login failed.');
//     } finally {
//       setLoading(false);
//     }
//   };

//   return (
//     <div className="relative min-h-[calc(100vh-4rem)] flex items-center justify-center bg-[#050505] overflow-hidden">
      
//       {/* 🟢 Galaxy Animation Background (Peeche chalega) */}
//       <div className="absolute inset-0 z-0">
//         <Galaxy className="opacity-80" density={1.2} glowIntensity={0.5} saturation={0.7} />
//       </div>

//       {/* Circular Glass Container (Aage dikhega) */}
//       <div className="relative z-10 flex items-center justify-center w-[400px] h-[400px] rounded-full bg-gradient-to-br from-white/10 to-black/60 backdrop-blur-md border border-white/20 shadow-[0_20px_50px_rgba(0,0,0,0.8)]">
        
//         {/* Neon Green Glow Ring */}
//         <div className="absolute w-[350px] h-[350px] rounded-full border-[3px] border-[#65a30d]/50 shadow-[0_0_30px_rgba(101,163,13,0.4),inset_0_0_30px_rgba(101,163,13,0.4)] pointer-events-none"></div>

//         {/* Form Content */}
//         <div className="relative z-20 w-full max-w-[240px] flex flex-col items-center">
//           <form onSubmit={handleSubmit} className="w-full space-y-4">
            
//             <div className="w-full">
//               <label className="block text-xs text-gray-300 mb-1 ml-2 font-semibold tracking-wider">USERNAME / EMAIL</label>
//               <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)}
//                 className="w-full bg-white/90 text-black px-4 py-2.5 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner font-medium" 
//                 placeholder="agent@support.com" 
//               />
//             </div>

//             <div className="w-full">
//               <label className="block text-xs text-gray-300 mb-1 ml-2 font-semibold tracking-wider">PASSWORD</label>
//               <input type="password" required value={password} onChange={(e) => setPassword(e.target.value)}
//                 className="w-full bg-white/90 text-black px-4 py-2.5 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner font-medium" 
//                 placeholder="••••••••" 
//               />
//             </div>

//             <div className="w-full pt-4 text-center">
//               <button type="submit" disabled={loading} 
//                 className="text-[#84cc16] text-2xl font-light tracking-widest hover:text-white hover:shadow-[0_0_15px_#84cc16] transition-all duration-300">
//                 {loading ? '...' : 'LOGIN'}
//               </button>
//             </div>
//           </form>

//           <p className="mt-8 text-xs text-gray-400">
//             No account? <Link to="/register" className="text-white hover:text-[#84cc16] hover:underline transition-colors tracking-wide">Register</Link>
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
// import { useAuth } from '../context/AuthContext';
// import Galaxy from './Galaxy';

// export default function Login() {
//   const [email, setEmail] = useState('');
//   const [password, setPassword] = useState('');
//   const [loading, setLoading] = useState(false);
//   const navigate = useNavigate();
//   const { login } = useAuth();

//   const handleSubmit = async (e) => {
//     e.preventDefault();
//     setLoading(true);
//     try {
//       const response = await axios.post('http://localhost:5215/api/v1/Auth/login', { email, password });
//        // const response = await axios.post('http://34.93.237.221:5215/api/v1/Auth/login', { email, password });
      
//       const { token, role } = response.data;
//       login(token, role);
//       toast.success('Logged in successfully!');
      
//   //     if (role === 'Agent' || role === 'Admin') {
//   //       navigate('/agent-dashboard');
//   //     } else {
//   //       navigate('/submit-ticket');
//   //     }
//   //   } catch (error) {
//   //     toast.error(error.response?.data?.message || error.response?.data || 'Login failed.');
//   //   } finally {
//   //     setLoading(false);
//   //   }
//   // };

//      // 🔥 UPDATED LOGIC: Agar Role 'Admin' hai toh Dashboard par bhejo, warna Ticket submit page par.
//       if (role === 'Admin') {
//         navigate('/agent-dashboard'); 
//       } else {
//         navigate('/submit-ticket');
//       }
//     } catch (error) {
//       toast.error(error.response?.data?.message || error.response?.data || 'Login failed.');
//     } finally {
//       setLoading(false);
//     }
//   };

//   return (
//     // 🟢 FIX: h-screen w-full kar diya taaki white space na aaye
//     <div className="relative h-screen w-full flex items-center justify-center bg-[#050505] overflow-hidden">
      
//       {/* Galaxy Animation Background */}
//       <div className="absolute inset-0 z-0 w-full h-full">
//         <Galaxy className="opacity-80" density={1.2} glowIntensity={0.5} saturation={0.7} />
//       </div>

//       {/* Circular Glass Container (Thoda bada aur spacious kiya) */}
//       <div className="relative z-10 flex items-center justify-center w-[440px] h-[440px] rounded-full bg-gradient-to-br from-white/10 to-black/60 backdrop-blur-md border border-white/20 shadow-[0_20px_50px_rgba(0,0,0,0.8)]">
        
//         {/* Neon Green Glow Ring */}
//         <div className="absolute w-[390px] h-[390px] rounded-full border-[3px] border-[#65a30d]/50 shadow-[0_0_40px_rgba(101,163,13,0.4),inset_0_0_40px_rgba(101,163,13,0.4)] pointer-events-none"></div>

//         {/* Form Content (Spacing aur padding badhai hai) */}
//         <div className="relative z-20 w-full max-w-[280px] flex flex-col items-center">
//           <form onSubmit={handleSubmit} className="w-full space-y-6"> {/* 🟢 space-y-6 kiya for better gap */}
            
//             <div className="w-full">
//               <label className="block text-xs text-gray-300 mb-1.5 ml-3 font-semibold tracking-wider">USERNAME / EMAIL</label>
//               <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)}
//                 className="w-full bg-white/90 text-black px-5 py-3 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner font-medium transition-all" 
//                 placeholder="agent@support.com" 
//               />
//             </div>

//             <div className="w-full">
//               <label className="block text-xs text-gray-300 mb-1.5 ml-3 font-semibold tracking-wider">PASSWORD</label>
//               <input type="password" required value={password} onChange={(e) => setPassword(e.target.value)}
//                 className="w-full bg-white/90 text-black px-5 py-3 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner font-medium transition-all" 
//                 placeholder="••••••••" 
//               />
//             </div>

//             <div className="w-full pt-4 text-center">
//               <button type="submit" disabled={loading} 
//                 className="text-[#84cc16] text-2xl font-light tracking-widest hover:text-white hover:shadow-[0_0_20px_#84cc16] transition-all duration-300">
//                 {loading ? '...' : 'LOGIN'}
//               </button>
//             </div>
//           </form>

//           <p className="mt-8 text-sm text-gray-400">
//             No account? <Link to="/register" className="text-white hover:text-[#84cc16] hover:underline transition-colors tracking-wide font-medium">Register</Link>
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
import { useAuth } from '../context/AuthContext';
import Galaxy from './Galaxy';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { login } = useAuth();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
               const response = await axios.post('http://localhost:5215/api/v1/Auth/login', { email, password });
             // const response = await axios.post('http://34.93.237.221:5215/api/v1/Auth/login', { email, password });

      
      const { token, role } = response.data;
      login(token, role);
      toast.success('Logged in successfully!');
      
      if (role === 'Admin') {
        navigate('/agent-dashboard'); 
      } else {
        navigate('/submit-ticket');
      }
    } catch (error) {
      toast.error(error.response?.data?.message || error.response?.data || 'Login failed.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="relative h-screen w-full flex items-center justify-center bg-[#050505] overflow-hidden">
      
      <div className="absolute inset-0 z-0 w-full h-full">
        <Galaxy className="opacity-80" density={1.2} glowIntensity={0.5} saturation={0.7} />
      </div>

      <div className="relative z-10 flex items-center justify-center w-[440px] h-[440px] rounded-full bg-gradient-to-br from-white/10 to-black/60 backdrop-blur-md border border-white/20 shadow-[0_20px_50px_rgba(0,0,0,0.8)]">
        
        <div className="absolute w-[390px] h-[390px] rounded-full border-[3px] border-[#65a30d]/50 shadow-[0_0_40px_rgba(101,163,13,0.4),inset_0_0_40px_rgba(101,163,13,0.4)] pointer-events-none"></div>

        <div className="relative z-20 w-full max-w-[280px] flex flex-col items-center">
          <form onSubmit={handleSubmit} className="w-full space-y-6"> 
            
            <div className="w-full">
              <label className="block text-xs text-gray-300 mb-1.5 ml-3 font-semibold tracking-wider">USERNAME / EMAIL</label>
              <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)}
                className="w-full bg-white/90 text-black px-5 py-3 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner font-medium transition-all" 
                placeholder="user@example.com" />
            </div>

            <div className="w-full">
              <label className="block text-xs text-gray-300 mb-1.5 ml-3 font-semibold tracking-wider">PASSWORD</label>
              <input type="password" required value={password} onChange={(e) => setPassword(e.target.value)}
                className="w-full bg-white/90 text-black px-5 py-3 rounded-full outline-none focus:ring-2 focus:ring-[#84cc16] shadow-inner font-medium transition-all" 
                placeholder="••••••••" />
            </div>

            <div className="w-full pt-4 text-center">
              <button type="submit" disabled={loading} 
                className="text-[#84cc16] text-2xl font-light tracking-widest hover:text-white hover:shadow-[0_0_20px_#84cc16] transition-all duration-300">
                {loading ? '...' : 'LOGIN'}
              </button>
            </div>
          </form>

          <p className="mt-8 text-sm text-gray-400">
            No account? <Link to="/register" className="text-white hover:text-[#84cc16] hover:underline transition-colors tracking-wide font-medium">Register</Link>
          </p>
        </div>
      </div>
    </div>
  );
}