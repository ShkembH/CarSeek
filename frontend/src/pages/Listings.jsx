
import React, { useState, useEffect, useCallback } from 'react';
import { useAuth } from '../context/AuthContext';
import { useNavigate, useLocation } from 'react-router-dom'; // Add useLocation
import { apiService } from '../services/api';
import '../styles/pages/Listings.css';
import carData from '../carData';

const Listings = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [carListings, setCarListings] = useState([]);
  const [filteredListings, setFilteredListings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showAdvancedFilters, setShowAdvancedFilters] = useState(false);
  const { isAuthenticated } = useAuth();

  // Filter states
  const [filters, setFilters] = useState({
    producer: '', // Brand
    series: '',   // Series/Family
    carClass: '', // Class/Body Type
    yearRange: '',
    kmRange: '',
    priceRange: '',
    yearFrom: '',
    yearTo: '',
    kmFrom: '',
    kmTo: '',
    priceFrom: '',
    priceTo: ''
  });



  // Add hardcoded brand/model options for filter dropdowns
  const mileageOptions = ['Any', '0-50,000', '50,001-100,000', '100,001-150,000', '150,001-200,000', '200,001+'];
  const yearOptions = ['Any', '2024', '2023', '2022', '2021', '2020', '2019', '2018', '2017', '2016', '2015', '2010-2014', '2005-2009', '2000-2004', 'Before 2000'];
  const priceOptions = ['Any', 'Up to €5,000', 'Up to €10,000', 'Up to €15,000', 'Up to €20,000', 'Up to €30,000', 'Up to €50,000', '€50,000+'];

  const applyFilters = useCallback(() => {
    let filtered = [...carListings];

    // Filter by producer
    if (filters.producer) {
      filtered = filtered.filter(car => car.make === filters.producer);
    }

    // Filter by model
    if (filters.model) {
      filtered = filtered.filter(car => car.model.toLowerCase().includes(filters.model.toLowerCase()));
    }

    // Filter by year range (dropdown)
    if (filters.yearRange) {
      const { min, max } = parseRange(filters.yearRange);
      if (min) filtered = filtered.filter(car => car.year >= min);
      if (max) filtered = filtered.filter(car => car.year <= max);
    }

    // Filter by year range (advanced)
    if (filters.yearFrom) {
      filtered = filtered.filter(car => car.year >= parseInt(filters.yearFrom));
    }
    if (filters.yearTo) {
      filtered = filtered.filter(car => car.year <= parseInt(filters.yearTo));
    }

    // Filter by mileage range (dropdown)
    if (filters.kmRange) {
      const { min, max } = parseRange(filters.kmRange);
      if (min) filtered = filtered.filter(car => car.mileage >= min);
      if (max) filtered = filtered.filter(car => car.mileage <= max);
    }

    // Filter by mileage range (advanced)
    if (filters.kmFrom) {
      filtered = filtered.filter(car => car.mileage >= parseInt(filters.kmFrom));
    }
    if (filters.kmTo) {
      filtered = filtered.filter(car => car.mileage <= parseInt(filters.kmTo));
    }

    // Filter by price range (dropdown)
    if (filters.priceRange) {
      const { min, max } = parseRange(filters.priceRange);
      if (min) filtered = filtered.filter(car => car.price >= min);
      if (max) filtered = filtered.filter(car => car.price <= max);
    }

    // Filter by price range (advanced)
    if (filters.priceFrom) {
      filtered = filtered.filter(car => car.price >= parseInt(filters.priceFrom));
    }
    if (filters.priceTo) {
      filtered = filtered.filter(car => car.price <= parseInt(filters.priceTo));
    }

    setFilteredListings(filtered);
  }, [filters, carListings]);

  useEffect(() => {
    fetchCarListings();

    // Handle URL parameters for filtering
    const searchParams = new URLSearchParams(location.search);
    const makeFilter = searchParams.get('make');
    const advanced = searchParams.get('advanced');

    if (makeFilter) {
      setFilters(prev => ({
        ...prev,
        producer: makeFilter
      }));
    }
    if (advanced === '1') {
      setShowAdvancedFilters(true);
    }

    // Auto-refresh every 60 seconds
    const interval = setInterval(() => {
      fetchCarListings();
    }, 60000);

    return () => clearInterval(interval);
  }, [location.search]);

  // Auto-apply filters when they change
  useEffect(() => {
    applyFilters();
  }, [applyFilters]);

  const fetchCarListings = async () => {
    try {
      console.log('🔍 [Listings] Fetching car listings from API...');
      const response = await apiService.getCarListings();
      console.log('📦 [Listings] Raw API response:', response);
      console.log('🚗 [Listings] Items array:', response.items);
      console.log('📊 [Listings] Items length:', response.items?.length || 0);

      if (!response.items) {
        console.warn('⚠️ [Listings] No items property in response, using empty array');
        setCarListings([]);
        setFilteredListings([]);
      } else {
        // Debug each listing's image data
        response.items.forEach((listing, index) => {
          console.log(`[Listings] Listing ${index + 1}:`, {
            id: listing.id,
            title: listing.title,
            primaryImageUrl: listing.primaryImageUrl,
            imagesCount: listing.images?.length || 0,
            images: listing.images
          });
        });

        setCarListings(response.items);
        setFilteredListings(response.items);
      }
      console.log('✅ [Listings] Car listings set in state');
    } catch (error) {
      console.error('❌ [Listings] Error fetching car listings:', error);
      setError('Unable to load car listings. Please try again later.');
    } finally {
      setLoading(false);
    }
  };

  const parseRange = (rangeString) => {
    if (!rangeString) return { min: null, max: null };

    if (rangeString.includes('+')) {
      const min = parseInt(rangeString.replace('+', ''));
      return { min, max: null };
    }

    const [min, max] = rangeString.split('-').map(val => parseInt(val.replace(/[,$]/g, '')));
    return { min: min || null, max: max || null };
  };

  const getAvailableSeries = () => {
    return filters.producer ? Object.keys(carData[filters.producer] || {}) : [];
  };
  const getAvailableClasses = () => {
    return filters.producer && filters.series ? carData[filters.producer][filters.series] || [] : [];
  };
  const handleFilterChange = (filterName, value) => {
    setFilters(prev => {
      if (filterName === 'producer') {
        return { ...prev, producer: value, series: '', carClass: '' };
      } else if (filterName === 'series') {
        return { ...prev, series: value, carClass: '' };
      } else {
        return { ...prev, [filterName]: value };
      }
    });
  };

  const clearFilters = () => {
    setFilters({
      producer: '',
      model: '',
      yearRange: '',
      kmRange: '',
      priceRange: '',
      yearFrom: '',
      yearTo: '',
      kmFrom: '',
      kmTo: '',
      priceFrom: '',
      priceTo: ''
    });
    setFilteredListings(carListings);
  };

  if (loading) {
    return (
      <div className="container mt-5 text-center">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
        <h2 className="mt-3">Loading car listings...</h2>
      </div>
    );
  }

  // Add this function to handle navigation
  const handleViewDetails = (carId) => {
    navigate(`/listings/${carId}`);
  };

  return (
    <div className="listings-main-container">
      {/* Sidebar Filter */}
      <aside className="listings-sidebar">
        <div className="card listings-filter-card">
          <form onSubmit={e => {e.preventDefault();applyFilters();}}>
            <div className="listings-filter-form">
              <select
                className="form-select form-select-sm"
                value={filters.producer}
                onChange={e => handleFilterChange('producer', e.target.value)}
              >
                <option value="">Brand</option>
                {Object.keys(carData).sort().map(brand => (
                  <option key={brand} value={brand}>{brand}</option>
                ))}
              </select>
              <select
                className="form-select form-select-sm"
                value={filters.series}
                onChange={e => handleFilterChange('series', e.target.value)}
                disabled={!filters.producer}
              >
                <option value="">{filters.producer ? 'Series' : 'First select a brand'}</option>
                {getAvailableSeries().map(series => (
                  <option key={series} value={series}>{series}</option>
                ))}
              </select>
              <select
                className="form-select form-select-sm"
                value={filters.carClass}
                onChange={e => handleFilterChange('carClass', e.target.value)}
                disabled={!filters.series || getAvailableClasses().length === 0}
              >
                <option value="">{filters.series ? 'Class/Body Type (optional)' : 'First select a series'}</option>
                {getAvailableClasses().map(carClass => (
                  <option key={carClass} value={carClass}>{carClass}</option>
                ))}
              </select>
              <select
                className="form-select form-select-sm"
                value={filters.kmRange}
                onChange={e => handleFilterChange('kmRange', e.target.value)}
              >
                {mileageOptions.map(opt => (
                  <option key={opt} value={opt}>{opt === 'Any' ? 'Mileage' : opt}</option>
                ))}
              </select>
              <select
                className="form-select form-select-sm"
                value={filters.yearRange}
                onChange={e => handleFilterChange('yearRange', e.target.value)}
              >
                {yearOptions.map(opt => (
                  <option key={opt} value={opt}>{opt === 'Any' ? 'Year from' : opt}</option>
                ))}
              </select>
              <select
                className="form-select form-select-sm"
                value={filters.priceRange}
                onChange={e => handleFilterChange('priceRange', e.target.value)}
              >
                {priceOptions.map(opt => (
                  <option key={opt} value={opt}>{opt === 'Any' ? 'Price up to' : opt}</option>
                ))}
              </select>
              <button type="submit" className="listings-btn" style={{marginBottom: 0}}>Search</button>
              <button type="button" className="listings-btn" onClick={clearFilters} style={{marginBottom: 0}}>Clear</button>

              {/* Advanced Filter Dropdown */}
              <div className="advanced-filter-dropdown">
                <button
                  type="button"
                  className="advanced-filter-toggle"
                  onClick={() => setShowAdvancedFilters(v => !v)}
                >
                  {showAdvancedFilters ? '▼ Advanced Filters' : '► Advanced Filters'}
                </button>
                {showAdvancedFilters && (
                  <div className="advanced-filter-fields">
                    <div className="advanced-filter-row">
                      <input
                        type="number"
                        className="form-control"
                        placeholder="Year From"
                        value={filters.yearFrom}
                        onChange={e => handleFilterChange('yearFrom', e.target.value)}
                        min="1900"
                        max="2024"
                      />
                      <input
                        type="number"
                        className="form-control"
                        placeholder="Year To"
                        value={filters.yearTo}
                        onChange={e => handleFilterChange('yearTo', e.target.value)}
                        min="1900"
                        max="2024"
                      />
                    </div>
                    <div className="advanced-filter-row">
                      <input
                        type="number"
                        className="form-control"
                        placeholder="KM From"
                        value={filters.kmFrom}
                        onChange={e => handleFilterChange('kmFrom', e.target.value)}
                        min="0"
                      />
                      <input
                        type="number"
                        className="form-control"
                        placeholder="KM To"
                        value={filters.kmTo}
                        onChange={e => handleFilterChange('kmTo', e.target.value)}
                        min="0"
                      />
                    </div>
                    <div className="advanced-filter-row">
                      <input
                        type="number"
                        className="form-control"
                        placeholder="Price From (€)"
                        value={filters.priceFrom}
                        onChange={e => handleFilterChange('priceFrom', e.target.value)}
                        min="0"
                      />
                      <input
                        type="number"
                        className="form-control"
                        placeholder="Price To (€)"
                        value={filters.priceTo}
                        onChange={e => handleFilterChange('priceTo', e.target.value)}
                        min="0"
                      />
                    </div>
                  </div>
                )}
              </div>
            </div>
          </form>
        </div>
      </aside>

      {/* Main Content: Car Cards */}
      <main className="listings-content">
        <div className="results-info d-flex justify-content-between align-items-center">
          <span className="fw-semibold text-muted">
            {filteredListings.length} cars found
          </span>
          <button
            className="btn btn-outline-secondary btn-sm"
            onClick={clearFilters}
          >
            Clear All Filters
          </button>
        </div>
        <div className="listings-cards-grid">
          {filteredListings.length === 0 ? (
            <div className="listings-empty-state">
              <div className="listings-empty-icon">🚗</div>
              <h4 className="text-muted">No cars found matching your criteria</h4>
              <p className="listings-empty-subtext">Try adjusting your filters or search terms</p>
            </div>
          ) : (
            filteredListings.map((car, index) => (
              <div key={index} className="listings-card-wrapper">
                <div
                  className="card listings-card"
                  onClick={() => handleViewDetails(car.id)}
                >
                  <img
                    src={car.primaryImageUrl || car.images?.[0]?.imageUrl || 'https://picsum.photos/300/200?random=1'}
                    className="listings-car-image"
                    alt={car.images?.[0]?.altText || `${car.make} ${car.model}`}
                  />
                  <div className="card-body d-flex flex-column">
                    <h5 className="card-title">{car.make} {car.model}</h5>
                    <p className="card-text text-muted">
                      {car.year} • {car.mileage?.toLocaleString()} km
                    </p>
                    <div className="mt-auto">
                      <div className="d-flex justify-content-between align-items-center">
                        <span className="h5 text-primary mb-0">
                          ${car.price?.toLocaleString()}
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </main>
    </div>
  );
};

export default Listings;
