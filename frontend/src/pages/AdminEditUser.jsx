import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { apiService } from '../services/api';
import { ProfileForm, DealershipProfileForm } from '../components/profile/ProfileForm';
import '../styles/pages/MyProfile.css';

const AdminEditUser = () => {
  const { userId } = useParams();
  const [user, setUser] = useState(null);
  const [listings, setListings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      try {
        const userData = await apiService.getUserById(userId);
        setUser(userData);
        const userListings = await apiService.getCarListingsByUser(userId);
        setListings(userListings);
      } catch (err) {
        setError('Failed to load user details.');
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [userId]);

  const navigate = useNavigate();

  const handleEditListing = (listingId) => {
    navigate(`/edit-listing/${listingId}`);
  };

  const handleDeleteListing = async (listingId) => {
    if (!window.confirm('Are you sure you want to delete this listing?')) return;
    try {
      await apiService.deleteCarListing(listingId);
      setListings(listings.filter(l => l.id !== listingId));
    } catch (err) {
      alert('Failed to delete listing.');
    }
  };

  const handleUpdate = async (form) => {
    setError(null);
    setSuccess(null);
    try {
      const data = { ...form };
      // Ensure isActive is a boolean
      if ('isActive' in data) {
        data.isActive = data.isActive === 'true' || data.isActive === true;
      }
      await apiService.updateUserByAdmin(userId, { request: data });
      setSuccess('User updated successfully!');
    } catch (err) {
      setError('Failed to update user.');
    }
  };

  if (loading) return <div className="container mt-5 text-center">Loading user details...</div>;
  if (error) return <div className="container mt-5 text-center text-danger">{error}</div>;
  if (!user) return <div className="container mt-5 text-center">User not found.</div>;

  return (
    <div className="my-profile-3col">
      {/* Left: Avatar, name, email, role, status */}
      <div className="my-profile-left">
        <div className="my-profile-avatar">
          <span>{(user.firstName?.[0] || user.companyName?.[0] || user.email?.[0] || 'U').toUpperCase()}</span>
        </div>
        <div className="my-profile-name">
          {user.firstName && user.lastName
            ? `${user.firstName} ${user.lastName}`
            : user.companyName || 'User'}
        </div>
        <div className="my-profile-email">{user.email}</div>
        <div style={{ marginTop: 12, fontSize: 14 }}>
          <div><strong>Role:</strong> {user.role}</div>
          <div><strong>Status:</strong> {user.isActive ? 'Active' : 'Inactive'}</div>
          <div><strong>Joined:</strong> {user.createdAt ? new Date(user.createdAt).toLocaleDateString() : 'N/A'}</div>
        </div>
      </div>
      {/* Center: Edit form */}
      <div className="my-profile-center">
        <h2>Edit User</h2>
        {success && <div className="my-profile-success">{success}</div>}
        {error && <div className="my-profile-error">{error}</div>}
        {(String(user.role).toLowerCase() === 'dealership' || user.role === 1) ? (
          <DealershipProfileForm profile={user} onUpdate={handleUpdate} />
        ) : (
          <ProfileForm profile={user} onUpdate={handleUpdate} />
        )}
      </div>
      {/* Right: User's listings */}
      <div className="my-profile-right" style={{ minWidth: 260, maxWidth: 340, overflowY: 'auto', background: '#fff', borderRadius: 16, boxShadow: '0 2px 12px rgba(0,0,0,0.06)', padding: 18 }}>
        <h4 style={{ marginBottom: 16, color: '#2563eb' }}>User's Listings</h4>
        {listings.length === 0 ? (
          <div className="text-muted">This user has no listings.</div>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
            {listings.map(listing => (
              <div key={listing.id} style={{ border: '1px solid #e5e7eb', borderRadius: 10, padding: 12, background: '#f8fafc' }}>
                <div style={{ fontWeight: 600 }}>{listing.title}</div>
                <div style={{ fontSize: 13, color: '#888' }}>{listing.year} {listing.make} {listing.model}</div>
                <div style={{ fontSize: 14, color: '#16a34a', fontWeight: 500 }}>${listing.price?.toLocaleString()}</div>
                <div style={{ fontSize: 12, color: '#888' }}>Status: <span style={{ color: listing.status === 'Active' ? '#16a34a' : '#eab308' }}>{listing.status}</span></div>
                <div style={{ fontSize: 12, color: '#888' }}>Created: {new Date(listing.createdAt).toLocaleDateString()}</div>
                <div style={{ display: 'flex', gap: 8, marginTop: 8 }}>
                  <button className="btn btn-outline-secondary btn-sm w-50" onClick={() => handleEditListing(listing.id)}>Edit</button>
                  <button className="btn btn-outline-danger btn-sm w-50" onClick={() => handleDeleteListing(listing.id)}>Delete</button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default AdminEditUser; 