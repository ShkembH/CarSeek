import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiService } from '../services/api';

const SavedListings = () => {
  const [listings, setListings] = useState([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    apiService.getSavedListings().then(res => {
      setListings(res);
      setLoading(false);
    });
  }, []);

  if (loading) return <div className="container mt-5 text-center"><div className="spinner-border text-primary" role="status"><span className="visually-hidden">Loading...</span></div></div>;

  if (!listings.length) return <div className="container mt-5 text-center"><h3>No saved listings yet.</h3></div>;

  return (
    <div className="container py-4">
      <h2 className="mb-4">Saved Listings</h2>
      <div className="row">
        {listings.map(listing => (
          <div className="col-md-4 mb-4" key={listing.id}>
            <div className="card h-100" style={{ cursor: 'pointer' }} onClick={() => navigate(`/listings/${listing.id}`)}>
              <img src={listing.primaryImageUrl || listing.images?.[0]?.imageUrl || '/api/placeholder/300/200'} className="card-img-top" alt={listing.title} style={{ height: 200, objectFit: 'cover' }} />
              <div className="card-body">
                <h5 className="card-title">{listing.title || `${listing.year} ${listing.make} ${listing.model}`}</h5>
                <p className="card-text">{listing.price ? `$${listing.price.toLocaleString()}` : ''}</p>
                <p className="card-text text-muted">{listing.year} {listing.make} {listing.model}</p>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default SavedListings;
