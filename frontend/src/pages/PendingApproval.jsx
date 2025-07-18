import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import '../styles/components/Auth.css';

const PendingApproval = () => {
  const location = useLocation();
  const navigate = useNavigate();
  
  // Get the approval message from URL query parameters
  const urlParams = new URLSearchParams(location.search);
  const approvalMessage = urlParams.get('message') || 
    "Your dealership account has been created successfully and is pending admin approval. You will be able to access your account once approved.";

  const handleGoHome = () => {
    navigate('/');
  };

  const handleContactSupport = () => {
    // You can implement this to open email or contact form
    window.open('mailto:support@carseek.com?subject=Dealership Approval Request', '_blank');
  };

  return (
    <div className="auth-container">
      <div className="auth-card" style={{ maxWidth: 600, textAlign: 'center' }}>
        <div style={{ fontSize: '4rem', marginBottom: '2rem' }}>⏳</div>
        
        <h2 className="auth-title" style={{ marginBottom: '1rem' }}>
          Account Pending Approval
        </h2>
        
        <div style={{ 
          background: '#f0f9ff', 
          border: '1px solid #0ea5e9', 
          borderRadius: '12px', 
          padding: '2rem', 
          marginBottom: '2rem',
          color: '#0c4a6e'
        }}>
          <p style={{ fontSize: '1.1rem', lineHeight: '1.6', marginBottom: '1rem' }}>
            {approvalMessage}
          </p>
          
          <p style={{ fontSize: '1rem', lineHeight: '1.5', opacity: 0.8 }}>
            Our team typically reviews and approves dealership accounts within 24-48 hours during business days.
          </p>
        </div>

        <div style={{ 
          background: '#fef3c7', 
          border: '1px solid #f59e0b', 
          borderRadius: '12px', 
          padding: '1.5rem', 
          marginBottom: '2rem',
          color: '#92400e'
        }}>
          <h4 style={{ marginBottom: '0.5rem', fontWeight: 600 }}>What happens next?</h4>
          <ul style={{ textAlign: 'left', margin: 0, paddingLeft: '1.5rem' }}>
            <li>You'll receive an email notification once your account is approved</li>
            <li>You can then log in and start listing your vehicles</li>
            <li>If you need to make changes to your application, please contact us</li>
          </ul>
        </div>

        <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center', flexWrap: 'wrap' }}>
          <button 
            className="auth-button" 
            onClick={handleGoHome}
            style={{ minWidth: '120px' }}
          >
            Go to Home
          </button>
          
          <button 
            className="auth-button" 
            onClick={handleContactSupport}
            style={{ 
              minWidth: '120px',
              background: 'linear-gradient(90deg, #dc2626 0%, #ef4444 100%)',
              border: 'none'
            }}
          >
            Contact Support
          </button>
        </div>

        <div className="auth-link" style={{ marginTop: '2rem' }}>
          Already have an approved account? <a href="/login">Login here</a>
        </div>
      </div>
    </div>
  );
};

export default PendingApproval; 