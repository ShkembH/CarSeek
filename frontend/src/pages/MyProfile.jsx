import React, { useEffect, useState } from 'react';
import { getProfile, updateProfile } from '../services/api';
import ProfileForm from '../components/profile/ProfileForm';
import '../styles/pages/MyProfile.css';

const MyProfile = () => {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);

  useEffect(() => {
    setLoading(true);
    getProfile()
      .then(data => {
        setProfile(data);
        setLoading(false);
      })
      .catch(err => {
        setError('Failed to load profile.');
        setLoading(false);
      });
  }, []);

  const handleUpdate = async (formData) => {
    setError(null);
    setSuccess(null);
    try {
      await updateProfile(formData);
      setSuccess('Profile updated successfully!');
      // Refetch profile
      const updated = await getProfile();
      setProfile(updated);
    } catch (err) {
      setError('Failed to update profile.');
    }
  };

  if (loading) return <div>Loading profile...</div>;
  if (error) return <div style={{ color: 'red' }}>{error}</div>;

  return (
    <div className="my-profile-3col">
      {/* Left: Avatar, name, email */}
      <div className="my-profile-left">
        <div className="my-profile-avatar">
          <span>{(profile.firstName?.[0] || profile.companyName?.[0] || 'U').toUpperCase()}</span>
        </div>
        <div className="my-profile-name">
          {profile.firstName && profile.lastName
            ? `${profile.firstName} ${profile.lastName}`
            : profile.companyName || 'User'}
        </div>
        <div className="my-profile-email">{profile.email}</div>
        {/* Dealership details */}
        {profile.role === 'Dealership' && (
          <div className="my-profile-dealership-details">
            <div><strong>Company Name:</strong> {profile.companyName}</div>
            <div><strong>Unique Number:</strong> {profile.companyUniqueNumber}</div>
            <div><strong>Location:</strong> {profile.location}</div>
            {profile.businessCertificatePath && (
              <div><strong>Certificate:</strong> <a href={profile.businessCertificatePath} target="_blank" rel="noopener noreferrer">View</a></div>
            )}
          </div>
        )}
      </div>
      {/* Center: Profile form */}
      <div className="my-profile-center">
        <h2>Profile Settings</h2>
        {success && <div className="my-profile-success">{success}</div>}
        {error && <div className="my-profile-error">{error}</div>}
        <ProfileForm profile={profile} onUpdate={handleUpdate} />
      </div>
      {/* Right: Placeholder for future use */}
      <div className="my-profile-right"></div>
    </div>
  );
};

export default MyProfile;
