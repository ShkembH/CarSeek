import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { apiService } from '../services/api';
import '../styles/pages/MyListings.css';

const MyListings = () => {
  const { isAuthenticated } = useAuth();
  const [listings, setListings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (isAuthenticated) {
      fetchMyListings();
    }
  }, [isAuthenticated]);

  const fetchMyListings = async () => {
    try {
      setLoading(true);
      const response = await apiService.getMyListings();
      console.log('My listings response:', response); // Add this line to debug
      // Check for both uppercase and lowercase property names
      setListings(response.Items || response.items || []);
    } catch (error) {
      console.error('Error fetching my listings:', error);
      setError('Failed to load your listings');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (listingId) => {
    if (window.confirm('Are you sure you want to delete this listing?')) {
      try {
        await apiService.deleteCarListing(listingId);
        setListings(listings.filter(listing => listing.id !== listingId));
      } catch (error) {
        console.error('Error deleting listing:', error);
        alert('Failed to delete listing');
      }
    }
  };

  if (!isAuthenticated) {
    return (
      <div className="container mt-5 text-center">
        <h2>Please log in to view your listings</h2>
        <a href="/login" className="btn btn-primary mt-3">Login</a>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="container mt-5 text-center">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
        <h2 className="mt-3">Loading your listings...</h2>
      </div>
    );
  }

  return (
    <div className="container mt-4">
      <div className="row">
        <div className="col-12">
          <div className="d-flex justify-content-between align-items-center mb-4">
            <h1>My Listings</h1>
            <a href="/add-listing" className="btn btn-primary">
              <i className="fas fa-plus me-2"></i>
              Add New Listing
            </a>
          </div>

          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          {listings.length === 0 ? (
            <div className="text-center py-5">
              <h3>No listings yet</h3>
              <p className="text-muted">You haven't created any car listings yet.</p>
              <a href="/add-listing" className="btn btn-primary">
                Create Your First Listing
              </a>
            </div>
          ) : (
            <div className="row">
              {listings.map(listing => (
                <div key={listing.id} className="col-md-6 col-lg-4 mb-4">
                  <div className="card h-100">
                    <div className="card-body">
                      <h5 className="card-title">{listing.title}</h5>
                      <p className="card-text text-muted">
                        {listing.year} {listing.make} {listing.model}
                      </p>
                      <p className="card-text">
                        <strong className="text-success">${listing.price.toLocaleString()}</strong>
                      </p>
                      <p className="card-text">
                        <small className="text-muted">
                          Status: <span className={`badge ${listing.status === 'Active' ? 'bg-success' : 'bg-warning'}`}>
                            {listing.status}
                          </span>
                        </small>
                      </p>
                      <p className="card-text">
                        <small className="text-muted">
                          Created: {new Date(listing.createdAt).toLocaleDateString()}
                        </small>
                      </p>
                    </div>
                    <div className="card-footer">
                      <div className="btn-group w-100">
                        <a href={`/listings/${listing.id}`} className="btn btn-outline-primary">
                          View
                        </a>
                        <button className="btn btn-outline-secondary">
                          Edit
                        </button>
                        <button
                          className="btn btn-outline-danger"
                          onClick={() => handleDelete(listing.id)}
                        >
                          Delete
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default MyListings;
