import React, { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import '../../styles/components/Auth.css';

const EyeIcon = ({ open }) => (
  open ? (
    <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#6b7280" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M1 12s4-7 11-7 11 7 11 7-4 7-11 7-11-7-11-7z"/><circle cx="12" cy="12" r="3"/></svg>
  ) : (
    <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#6b7280" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M17.94 17.94A10.94 10.94 0 0 1 12 19c-7 0-11-7-11-7a21.81 21.81 0 0 1 5.06-6.06"/><path d="M1 1l22 22"/><path d="M9.53 9.53A3 3 0 0 0 12 15a3 3 0 0 0 2.47-5.47"/><path d="M14.47 14.47A3 3 0 0 1 12 9a3 3 0 0 1-2.47 5.47"/></svg>
  )
);

const Register = () => {
  const [step, setStep] = useState(1);
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    confirmEmail: '',
    phoneNumber: '',
    country: '',
    city: '',
    password: '',
    confirmPassword: '',
    role: null,
    companyName: '',
    companyUniqueNumber: '',
    businessCertificate: null,
    location: '',
  });
  const [isLoading, setIsLoading] = useState(false);
  const [validationError, setValidationError] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const { register, error, clearError } = useAuth();

  const cities = {
    Kosovo: [
      'Pristina', 'Prizren', 'Peja', 'Gjakova', 'Gjilan', 'Mitrovica', 'Ferizaj', 'Vushtrri',
      'Podujeva', 'Suhareka', 'Rahovec', 'Lipjan', 'Malisheva', 'Kamenica', 'Viti', 'Decan',
      'Istog', 'Kllokot', 'Novoberde', 'Obiliq', 'Partesh', 'Ranillug', 'Gracanica',
      'Hani i Elezit', 'Mamusa', 'Junik', 'Zvecan', 'Zubin Potok', 'Leposavic', 'Mitrovica e Veriut'
    ],
    Albania: [
      'Tirana', 'Durres', 'Vlora', 'Elbasan', 'Shkoder', 'Fier', 'Korce', 'Berat', 'Lushnje',
      'Kavaja', 'Pogradec', 'Gjirokaster', 'Saranda', 'Lac', 'Kukes', 'Lezha', 'Patos',
      'Corovoda', 'Puke', 'Burrel', 'Kruja', 'Fushe-Kruja', 'Mamurras', 'Laç', 'Rubik',
      'Bulqiza', 'Klos', 'Mat', 'Dibra', 'Librazhd', 'Prrenjas', 'Belsh', 'Gramsh', 'Cërrik',
      'Peqin', 'Roskovec', 'Ballsh', 'Mallakaster', 'Divjaka', 'Lushnja', 'Libofsha',
      'Tepelena', 'Memaliaj', 'Kelcyra', 'Permet', 'Leskovik', 'Erseka', 'Maliq', 'Bilisht',
      'Devoll', 'Pustec', 'Lin', 'Finiq', 'Delvina', 'Konispol', 'Dropull', 'Himara',
      'Selenica', 'Orikum', 'Vau i Dejes', 'Koplik', 'Malesi e Madhe', 'Bajram Curri',
      'Has', 'Tropoja', 'Fushe Arrez', 'Rreshen'
    ]
  };

  const handleChange = (e) => {
    const { name, value, files } = e.target;
    if (name === 'country') {
      setFormData(prev => ({ ...prev, [name]: value, city: '' }));
    } else if (name === 'businessCertificate') {
      setFormData(prev => ({ ...prev, businessCertificate: files[0] }));
    } else {
      setFormData(prev => ({ ...prev, [name]: value }));
    }
    clearError();
    setValidationError('');
  };

  const handleAccountTypeSelect = (roleValue) => {
    setFormData(prev => ({ ...prev, role: roleValue }));
    setStep(2);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (formData.role === 1) {
      // Dealership: validate confirm email, business certificate, and file type
      if (formData.email !== formData.confirmEmail) {
        setValidationError('Emails do not match');
        return;
      }
      if (!formData.businessCertificate) {
        setValidationError('Business Certificate is required');
        return;
      }
      const allowedTypes = ['application/pdf', 'image/png', 'image/jpeg', 'image/jpg'];
      if (!allowedTypes.includes(formData.businessCertificate.type)) {
        setValidationError('Business Certificate must be a PDF or image file');
        return;
      }
    }
    if (formData.password !== formData.confirmPassword) {
      setValidationError('Passwords do not match');
      return;
    }
    if (formData.password.length < 6) {
      setValidationError('Password must be at least 6 characters long');
      return;
    }
    setIsLoading(true);
    const { confirmPassword, confirmEmail, ...userData } = formData;
    const result = await register(userData);
    if (result.success) {
      // Check if this is a dealership registration that requires approval
      if (result.requiresApproval) {
        // Redirect to pending approval page with the approval message
        window.location.href = `/pending-approval?message=${encodeURIComponent(result.approvalMessage || '')}`;
      } else {
        // Individual users go directly to home page
        window.location.href = '/';
      }
    }
    setIsLoading(false);
  };

  const availableCities = formData.country ? cities[formData.country] || [] : [];

  if (step === 1) {
    return (
      <div className="auth-container">
        <div className="auth-card auth-card-wide">
          <h2 className="auth-title">Join CarSeek</h2>
          <p className="auth-link" style={{ marginBottom: '2rem', color: '#6b7280', fontSize: '1.1rem' }}>Choose your account type to get started</p>
          <div className="auth-account-type-row">
            <div className="account-type-card" onClick={() => handleAccountTypeSelect(0)}>
              <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>👤</div>
              <h3 style={{ color: '#1f2937', marginBottom: '0.5rem', fontSize: '1.5rem' }}>Individual User</h3>
              <p style={{ color: '#6b7280', marginBottom: '1rem', fontSize: '1rem' }}>Perfect for car buyers and enthusiasts</p>
              <ul style={{ listStyle: 'none', padding: 0, margin: '1rem 0', textAlign: 'left' }}>
                <li>Browse thousands of cars</li>
                <li>Save favorite listings</li>
                <li>Contact dealers directly</li>
                <li>Get personalized recommendations</li>
              </ul>
              <button className="auth-button">Choose Individual</button>
            </div>
            <div className="account-type-card" onClick={() => handleAccountTypeSelect(1)}>
              <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>🏢</div>
              <h3 style={{ color: '#1f2937', marginBottom: '0.5rem', fontSize: '1.5rem' }}>Dealership</h3>
              <p style={{ color: '#6b7280', marginBottom: '1rem', fontSize: '1rem' }}>For car dealers and businesses</p>
              <ul style={{ listStyle: 'none', padding: 0, margin: '1rem 0', textAlign: 'left' }}>
                <li>List unlimited cars</li>
                <li>Instant listing approval</li>
                <li>Advanced analytics</li>
                <li>Priority customer support</li>
              </ul>
              <button className="auth-button">Choose Dealership</button>
            </div>
          </div>
          <div className="auth-link">
            Already have an account? <a href="/login">Login here</a>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-container">
      <div className="auth-card" style={{ maxWidth: 600 }}>
        <div style={{ display: 'flex', alignItems: 'center', marginBottom: '1.5rem', position: 'relative' }}>
          <button
            style={{ background: 'none', border: 'none', color: '#2563eb', cursor: 'pointer', fontSize: '1rem', padding: '0.5rem', position: 'absolute', left: 0 }}
            onClick={() => setStep(1)}
          >
            ← Back
          </button>
          <h2 className="auth-title" style={{ flex: 1, textAlign: 'center', margin: 0 }}>
            {formData.role === 0 ? 'Individual' : 'Dealership'} Registration
          </h2>
        </div>
        {(error || validationError) && (
          <div className="auth-error">
            {validationError
              ? validationError
              : (() => {
                  // Handle array or string error
                  let errMsg = error;
                  if (Array.isArray(error)) {
                    errMsg = error.join(' ');
                  }
                  if (typeof errMsg === 'string' && errMsg.includes('\n')) {
                    errMsg = errMsg.split('\n').join(' ');
                  }
                  const lower = (errMsg || '').toLowerCase();
                  if (
                    lower.includes('required') ||
                    lower.includes('must not be empty') ||
                    lower.includes('is required')
                  ) {
                    return 'Please fill in all required fields.';
                  }
                  if (
                    lower.includes('password must') ||
                    lower.includes('uppercase') ||
                    lower.includes('lowercase') ||
                    lower.includes('number')
                  ) {
                    return 'Password must be at least 6 characters and contain at least one uppercase letter, one lowercase letter, and one number.';
                  }
                  if (lower.includes('401')) {
                    return 'Registration failed. Please check your details and try again.';
                  }
                  if (lower.includes('400') && errMsg && errMsg !== 'HTTP error! status: 400') {
                    return errMsg;
                  }
                  // Remove the generic 400 message. Only show if no error message at all.
                  return errMsg;
                })()
            }
          </div>
        )}
        <form onSubmit={handleSubmit} className="auth-form">
          {formData.role === 1 ? (
            <>
              <div className="auth-field">
                <label className="auth-label">Company Name</label>
                <input
                  type="text"
                  name="companyName"
                  value={formData.companyName}
                  onChange={handleChange}
                  required={formData.role === 1}
                  className="auth-input"
                  placeholder="Company Name"
                />
              </div>
              <div className="auth-field">
                <label className="auth-label">Company Unique Number</label>
                <input
                  type="text"
                  name="companyUniqueNumber"
                  value={formData.companyUniqueNumber}
                  onChange={handleChange}
                  required={formData.role === 1}
                  className="auth-input"
                  placeholder="Unique Number"
                />
              </div>
              <div className="auth-field">
                <label className="auth-label">Business Certificate (PDF or Image)</label>
                <input
                  type="file"
                  name="businessCertificate"
                  accept=".pdf,image/png,image/jpeg,image/jpg"
                  onChange={handleChange}
                  required={formData.role === 1}
                  className="auth-input"
                />
              </div>
              <div className="auth-field">
                <label className="auth-label">Location</label>
                <input
                  type="text"
                  name="location"
                  value={formData.location}
                  onChange={handleChange}
                  required={formData.role === 1}
                  className="auth-input"
                  placeholder="Location"
                />
              </div>
              <div className="auth-field">
                <label className="auth-label">Email</label>
                <input
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  required
                  className="auth-input"
                  placeholder="Email"
                />
              </div>
              <div className="auth-field">
                <label className="auth-label">Confirm Email</label>
                <input
                  type="email"
                  name="confirmEmail"
                  value={formData.confirmEmail}
                  onChange={handleChange}
                  required={formData.role === 1}
                  className="auth-input"
                  placeholder="Confirm Email"
                />
              </div>
              <div className="auth-field">
                <label className="auth-label">Phone Number</label>
                <input
                  type="tel"
                  name="phoneNumber"
                  value={formData.phoneNumber}
                  onChange={handleChange}
                  required
                  className="auth-input"
                  placeholder="Phone Number"
                />
              </div>
              <div className="auth-field">
                <label className="auth-label">Country</label>
                <select
                  name="country"
                  value={formData.country}
                  onChange={handleChange}
                  required
                  className="auth-input"
                >
                  <option value="">Select Country</option>
                  <option value="Kosovo">Kosovo</option>
                  <option value="Albania">Albania</option>
                </select>
              </div>
              <div className="auth-field">
                <label className="auth-label">City</label>
                <select
                  name="city"
                  value={formData.city}
                  onChange={handleChange}
                  required
                  className="auth-input"
                >
                  <option value="">Select City</option>
                  {availableCities.map(city => (
                    <option key={city} value={city}>{city}</option>
                  ))}
                </select>
              </div>
            </>
          ) : (
            <>
              <div className="auth-field">
                <label className="auth-label">First Name</label>
                <input
                  type="text"
                  name="firstName"
                  value={formData.firstName}
                  onChange={handleChange}
                  required
                  className="auth-input"
                  placeholder="First Name"
                />
              </div>
              <div className="auth-field">
                <label className="auth-label">Last Name</label>
                <input
                  type="text"
                  name="lastName"
                  value={formData.lastName}
                  onChange={handleChange}
                  required
                  className="auth-input"
                  placeholder="Last Name"
                />
              </div>
              <div className="auth-field">
                <label className="auth-label">Email</label>
                <input
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  required
                  className="auth-input"
                  placeholder="Email"
                />
              </div>
              <div className="auth-field">
                <label className="auth-label">Phone Number</label>
                <input
                  type="tel"
                  name="phoneNumber"
                  value={formData.phoneNumber}
                  onChange={handleChange}
                  required
                  className="auth-input"
                  placeholder="Phone Number"
                />
              </div>
              <div className="auth-field">
                <label className="auth-label">Country</label>
                <select
                  name="country"
                  value={formData.country}
                  onChange={handleChange}
                  required
                  className="auth-input"
                >
                  <option value="">Select Country</option>
                  <option value="Kosovo">Kosovo</option>
                  <option value="Albania">Albania</option>
                </select>
              </div>
              <div className="auth-field">
                <label className="auth-label">City</label>
                <select
                  name="city"
                  value={formData.city}
                  onChange={handleChange}
                  required
                  className="auth-input"
                >
                  <option value="">Select City</option>
                  {availableCities.map(city => (
                    <option key={city} value={city}>{city}</option>
                  ))}
                </select>
              </div>
            </>
          )}
          {/* Common fields for both roles */}
          <div className="auth-field">
            <label className="auth-label">Password</label>
            <div className="auth-password-input">
              <input
                type={showPassword ? 'text' : 'password'}
                name="password"
                value={formData.password}
                onChange={handleChange}
                required
                className="auth-input"
                placeholder="Password"
              />
              <span onClick={() => setShowPassword(p => !p)} style={{ cursor: 'pointer', marginLeft: 8 }}>
                <EyeIcon open={showPassword} />
              </span>
            </div>
          </div>
          <div className="auth-field">
            <label className="auth-label">Confirm Password</label>
            <div className="auth-password-input">
              <input
                type={showConfirmPassword ? 'text' : 'password'}
                name="confirmPassword"
                value={formData.confirmPassword}
                onChange={handleChange}
                required
                className="auth-input"
                placeholder="Confirm Password"
              />
              <span onClick={() => setShowConfirmPassword(p => !p)} style={{ cursor: 'pointer', marginLeft: 8 }}>
                <EyeIcon open={showConfirmPassword} />
              </span>
            </div>
          </div>
          <button className="auth-button" type="submit" disabled={isLoading} style={{ marginTop: 18 }}>
            {isLoading ? 'Registering...' : 'Register'}
          </button>
        </form>
        <div className="auth-link">
          Already have an account? <a href="/login">Login here</a>
        </div>
      </div>
    </div>
  );
};

export default Register;
