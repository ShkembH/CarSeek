import React, { useState, useEffect } from 'react';

const ProfileForm = ({ profile, onUpdate }) => {
  const [form, setForm] = useState({ ...profile });
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    setForm({ ...profile });
  }, [profile]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    await onUpdate(form);
    setSubmitting(false);
  };

  return (
    <form className="profile-form" onSubmit={handleSubmit}>
      <div className="profile-form-group">
        <label htmlFor="email">Email</label>
        <input type="email" id="email" name="email" value={form.email || ''} disabled />
      </div>
      <div className="profile-form-group">
        <label htmlFor="phoneNumber">Phone Number</label>
        <input type="text" id="phoneNumber" name="phoneNumber" value={form.phoneNumber || ''} onChange={handleChange} />
      </div>
      <div className="profile-form-group">
        <label htmlFor="country">Country</label>
        <input type="text" id="country" name="country" value={form.country || ''} onChange={handleChange} />
      </div>
      <div className="profile-form-group">
        <label htmlFor="city">City</label>
        <input type="text" id="city" name="city" value={form.city || ''} onChange={handleChange} />
      </div>
      <div className="profile-form-group">
        <label htmlFor="firstName">First Name</label>
        <input type="text" id="firstName" name="firstName" value={form.firstName || ''} onChange={handleChange} />
      </div>
      <div className="profile-form-group">
        <label htmlFor="lastName">Last Name</label>
        <input type="text" id="lastName" name="lastName" value={form.lastName || ''} onChange={handleChange} />
      </div>
      <div className="profile-form-group">
        <label htmlFor="isActive">Active</label>
        <select
          id="isActive"
          name="isActive"
          value={form.isActive ? 'true' : 'false'}
          onChange={handleChange}
        >
          <option value="true">Active</option>
          <option value="false">Inactive</option>
        </select>
      </div>
      <button type="submit" disabled={submitting}>{submitting ? 'Saving...' : 'Save Changes'}</button>
    </form>
  );
};

const DealershipProfileForm = ({ profile, onUpdate }) => {
  const [form, setForm] = useState({ ...profile });
  const [file, setFile] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    setForm({ ...profile });
  }, [profile]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleFileChange = (e) => {
    setFile(e.target.files[0]);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    await onUpdate({ ...form, businessCertificate: file });
    setSubmitting(false);
  };

  return (
    <form className="profile-form" onSubmit={handleSubmit}>
      <div className="profile-form-group">
        <label htmlFor="email">Email</label>
        <input type="email" id="email" name="email" value={form.email || ''} disabled />
      </div>
      <div className="profile-form-group">
        <label htmlFor="companyName">Company Name</label>
        <input type="text" id="companyName" name="companyName" value={form.companyName || ''} onChange={handleChange} />
      </div>
      <div className="profile-form-group">
        <label htmlFor="companyUniqueNumber">Company Unique Number</label>
        <input type="text" id="companyUniqueNumber" name="companyUniqueNumber" value={form.companyUniqueNumber || ''} onChange={handleChange} />
      </div>
      <div className="profile-form-group">
        <label htmlFor="location">Location</label>
        <input type="text" id="location" name="location" value={form.location || ''} onChange={handleChange} />
      </div>
      <div className="profile-form-group">
        <label htmlFor="businessCertificate">Business Certificate</label>
        <input type="file" id="businessCertificate" name="businessCertificate" accept=".pdf,image/*" onChange={handleFileChange} />
        {profile.businessCertificatePath && (
          <a href={profile.businessCertificatePath} target="_blank" rel="noopener noreferrer">View Current Certificate</a>
        )}
      </div>
      <div className="profile-form-group">
        <label htmlFor="isActive">Active</label>
        <select
          id="isActive"
          name="isActive"
          value={form.isActive ? 'true' : 'false'}
          onChange={handleChange}
        >
          <option value="true">Active</option>
          <option value="false">Inactive</option>
        </select>
      </div>
      <button type="submit" disabled={submitting}>{submitting ? 'Saving...' : 'Save Changes'}</button>
    </form>
  );
};

export { ProfileForm, DealershipProfileForm };
