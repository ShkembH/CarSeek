import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import Header from './components/common/Header';
import Footer from './components/common/Footer'; // Add this import
import Home from './pages/Home';
import Listings from './pages/Listings';
import AddListing from './pages/AddListing';
import Dashboard from './pages/Dashboard';
import MyListings from './pages/MyListings';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import CarDetails from './pages/CarDetails';
import ProtectedRoute from './components/ProtectedRoute';
import AdminRoute from './components/AdminRoute';
import SavedListings from './pages/SavedListings';
import MyProfile from './pages/MyProfile';
import Inbox from './pages/Inbox';
import PendingApproval from './pages/PendingApproval';
import './App.css';

function App() {
  return (
    <AuthProvider>
      <Router>
        <div className="App">
          <Header />
          <main>
            <Routes>
              <Route path="/" element={<Home />} />
              <Route path="/listings" element={<Listings />} /> {/* No auth needed */}
              <Route path="/listings/:id" element={<CarDetails />} /> {/* No auth needed */}
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/pending-approval" element={<PendingApproval />} />
              <Route path="/saved-listings" element={<SavedListings />} />
              <Route path="/profile" element={
                <ProtectedRoute>
                  <MyProfile />
                </ProtectedRoute>
              } />
              <Route path="/inbox" element={<Inbox />} />

              {/* Protected routes for sellers */}
              <Route path="/add-listing" element={
                <ProtectedRoute>
                  <AddListing />
                </ProtectedRoute>
              } />
              <Route path="/my-listings" element={
                <ProtectedRoute>
                  <MyListings />
                </ProtectedRoute>
              } />

              {/* Admin only routes */}
              <Route path="/dashboard" element={
                <AdminRoute>
                  <Dashboard />
                </AdminRoute>
              } />
            </Routes>
          </main>
          <Footer /> {/* Add the Footer component here */}
        </div>
      </Router>
    </AuthProvider>
  );
}

export default App;
