import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { apiService } from '../services/api';
import '../styles/pages/MyProfile.css';

const DealershipUserDetails = ({ user }) => (
  <form className="profile-form" style={{ pointerEvents: 'none' }}>
    <div className="profile-form-group">
      <label>Email</label>
      <input type="email" value={user.email || ''} disabled />
    </div>
    <div className="profile-form-group">
      <label>Company Name</label>
      <input type="text" value={user.companyName || ''} disabled />
    </div>
    <div className="profile-form-group">
      <label>Company Unique Number</label>
      <input type="text" value={user.companyUniqueNumber || ''} disabled />
    </div>
    <div className="profile-form-group">
      <label>Location</label>
      <input type="text" value={user.location || ''} disabled />
    </div>
    <div className="profile-form-group">
      <label>Phone Number</label>
      <input type="text" value={user.dealershipPhoneNumber || ''} disabled />
    </div>
    <div className="profile-form-group">
      <label>Website</label>
      <input type="text" value={user.website || ''} disabled />
    </div>
    <div className="profile-form-group">
      <label>Business Certificate</label>
      {user.businessCertificatePath ? (
        <a href={user.businessCertificatePath} target="_blank" rel="noopener noreferrer">View Certificate</a>
      ) : (
        <input type="text" value={user.businessCertificatePath || ''} disabled />
      )}
    </div>
    <div className="profile-form-group">
      <label>Description</label>
      <input type="text" value={user.description || ''} disabled />
    </div>
    <div className="profile-form-group">
      <label>Address</label>
      <input type="text" value={user.addressStreet || ''} disabled placeholder="Street" />
      <input type="text" value={user.addressCity || ''} disabled placeholder="City" />
      <input type="text" value={user.addressState || ''} disabled placeholder="State" />
      <input type="text" value={user.addressPostalCode || ''} disabled placeholder="Postal Code" />
      <input type="text" value={user.addressCountry || ''} disabled placeholder="Country" />
    </div>
    <div className="profile-form-group">
      <label>Approval Status</label>
      <input type="text" value={user.isDealershipApproved ? 'Approved' : 'Not Approved'} disabled />
    </div>
  </form>
);

const AdminUserDetails = () => {
  const { userId } = useParams();
  const [user, setUser] = useState(null);
  const [listings, setListings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      try {
        const userData = await apiService.getUserById(userId);
        const userListings = await apiService.getCarListingsByUser(userId);
        setUser(userData);
        setListings(userListings);
      } catch (err) {
        setError('Failed to load user details.');
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [userId]);

  if (loading) return <div className="container mt-5 text-center">Loading user details...</div>;
  if (error) return <div className="container mt-5 text-center text-danger">{error}</div>;
  if (!user) return <div className="container mt-5 text-center">User not found.</div>;

  // Helper: show value or dash
  const show = (val) => val || <span style={{ color: '#bbb' }}>—</span>;

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
      {/* Center: User info (individual or dealership) */}
      <div className="my-profile-center">
        <h2>User Info</h2>
        {(String(user.role).toLowerCase() === 'dealership' || user.role === 1) ? (
          <DealershipUserDetails user={user} />
        ) : (
          <form className="profile-form" style={{ pointerEvents: 'none' }}>
            <div className="profile-form-group">
              <label>Email</label>
              <input type="email" value={show(user.email)} disabled />
            </div>
            <div className="profile-form-group">
              <label>Phone Number</label>
              <input type="text" value={show(user.phoneNumber)} disabled />
            </div>
            <div className="profile-form-group">
              <label>Country</label>
              <input type="text" value={show(user.country)} disabled />
            </div>
            <div className="profile-form-group">
              <label>City</label>
              <input type="text" value={show(user.city)} disabled />
            </div>
            <div className="profile-form-group">
              <label>First Name</label>
              <input type="text" value={show(user.firstName)} disabled />
            </div>
            <div className="profile-form-group">
              <label>Last Name</label>
              <input type="text" value={show(user.lastName)} disabled />
            </div>
          </form>
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
                <a href={`/listings/${listing.id}`} className="btn btn-outline-primary btn-sm mt-2 w-100">View Listing</a>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default AdminUserDetails; 