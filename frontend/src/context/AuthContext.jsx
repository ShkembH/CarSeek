import React, { createContext, useContext, useState, useEffect } from 'react';
import { apiService } from '../services/api';

const AuthContext = createContext();

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    // Check if user is logged in on app start
    const token = localStorage.getItem('token');
    const userData = localStorage.getItem('user');

    if (token && userData && userData !== 'undefined') {
      try {
        setUser(JSON.parse(userData));
      } catch (error) {
        console.error('Error parsing user data from localStorage:', error);
        // Clear invalid data
        localStorage.removeItem('token');
        localStorage.removeItem('user');
      }
    }
    setLoading(false);
  }, []);

  const login = async (email, password) => {
    try {
      setLoading(true);
      setError(null);

      const response = await apiService.login(email, password);
      console.log('🔐 [AuthContext] Login response:', response);

      // Check if this is a dealership user that needs approval
      if (response.role === 1 || response.role === 'Dealership') {
        // For dealership users, we need to check if they're approved
        // This would require an additional API call to check approval status
        // For now, we'll allow login but the backend should handle this
        console.log('🏢 [AuthContext] Dealership user logging in');
      }

      // Create user object from response
      const userData = {
        id: response.id,
        email: response.email,
        firstName: response.firstName,
        lastName: response.lastName,
        role: response.role
      };

      console.log('👤 [AuthContext] User data created:', userData);

      // Store token and user data
      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify(userData));

      setUser(userData);
      return { success: true };
    } catch (error) {
      setError(error.message);
      return { success: false, error: error.message };
    } finally {
      setLoading(false);
    }
  };

  const register = async (userData) => {
    try {
      setLoading(true);
      setError(null);

      const response = await apiService.register(userData);
      console.log('🔐 [AuthContext] Register response:', response);

      // Check if approval is required
      if (response.requiresApproval) {
        // Don't set user as logged in if approval is required
        return { 
          success: true, 
          requiresApproval: true, 
          approvalMessage: response.approvalMessage 
        };
      }

      // Create user object from response (only for immediate access)
      const userObj = {
        id: response.id,
        email: response.email,
        firstName: response.firstName,
        lastName: response.lastName,
        role: response.role
      };

      console.log('👤 [AuthContext] User object created:', userObj);

      // Store token and user data (only for immediate access)
      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify(userObj));

      setUser(userObj);
      return { success: true };
    } catch (error) {
      setError(error.message);
      return { success: false, error: error.message };
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
    setError(null);
  };

  const clearError = () => {
    setError(null);
  };

  const value = {
    user,
    loading,
    error,
    login,
    register,
    logout,
    clearError,
    isAuthenticated: !!user,
    isDealer: user?.role === 1 || user?.role === 'Dealership', // UserRole.Dealership = 1
    isAdmin: user?.role === 2 || user?.role === 'Admin',  // UserRole.Admin = 2
    isUser: user?.role === 0 || user?.role === 'Individual'    // UserRole.Individual = 0
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};
