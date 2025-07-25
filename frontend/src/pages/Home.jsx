import React, { useState, useEffect, useRef } from 'react';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { apiService } from '../services/api';
// Import Swiper components and styles
import { Swiper, SwiperSlide } from 'swiper/react';
import { Navigation, Autoplay } from 'swiper/modules';
import 'swiper/css';
import 'swiper/css/navigation';
import 'swiper/css/pagination';
import brandLogos from '../assets/brands';
import '../styles/pages/Home.css';

const Home = () => {
  const navigate = useNavigate();
  const [carListings, setCarListings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const { isAuthenticated, user } = useAuth();

  // Swiper navigation refs
  const brandsPrevRef = useRef(null);
  const brandsNextRef = useRef(null);
  const carsPrevRef = useRef(null);
  const carsNextRef = useRef(null);
  // Search form state (move here to avoid conditional hook call)
  const [selectedBrand, setSelectedBrand] = useState('');
  const [selectedModel, setSelectedModel] = useState('');
  const [selectedMileage, setSelectedMileage] = useState('Any');
  const [selectedYear, setSelectedYear] = useState('Any');
  const [selectedPrice, setSelectedPrice] = useState('Any');

  useEffect(() => {
    fetchCarListings();

    // Listen for listing updates from admin dashboard
    const handleStorageChange = (e) => {
      if (e.key === 'listingsUpdated') {
        fetchCarListings();
      }
    };

    window.addEventListener('storage', handleStorageChange);
    return () => window.removeEventListener('storage', handleStorageChange);
  }, []);

  const fetchCarListings = async () => {
    try {
      console.log('🔍 Fetching car listings from API...');
      const response = await apiService.getCarListings();
      console.log('📦 Raw API response:', response);
      console.log('🔍 Response keys:', Object.keys(response));
      console.log('🚗 Items array:', response.items);
      console.log('📊 Items length:', response.items?.length || 0);

      setCarListings(response.items || []);
      console.log('✅ Car listings set in state');
    } catch (error) {
      console.error('❌ Error fetching car listings:', error);
      setError('Unable to load car listings. Please try again later.');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="loading-container">
        <h2>Loading car listings...</h2>
      </div>
    );
  }

  // Add this function to handle navigation
  const handleViewDetails = (carId) => {
    navigate(`/listings/${carId}`);
  };

  // Handle brand filter navigation
  const handleBrandClick = (brand) => {
    navigate(`/listings?make=${encodeURIComponent(brand)}`);
  };

  // Handler for Advanced Search click
  const handleAdvancedSearch = (e) => {
    e.preventDefault();
    navigate('/listings?advanced=1'); // Pass a query param to open advanced filter
  };

  // Hardcoded brands and models
  const brandOptions = [
    {
      name: 'Toyota',
      models: ['Corolla', 'Camry', 'RAV4', 'Yaris', 'Prius', 'Highlander', 'Land Cruiser', 'C-HR', 'Avensis', 'Auris', 'Verso', 'Aygo', 'Supra', 'Celica', 'Hilux']
    },
    {
      name: 'BMW',
      models: ['3 Series', '5 Series', '7 Series', 'X1', 'X3', 'X5', 'X6', 'X7', 'M3', 'M4', 'M5', 'Z4', 'i3', 'i8', '2 Series']
    },
    {
      name: 'Mercedes-Benz',
      models: ['A-Class', 'B-Class', 'C-Class', 'E-Class', 'S-Class', 'GLA', 'GLC', 'GLE', 'GLS', 'CLA', 'CLS', 'SLK', 'G-Class', 'Vito', 'Sprinter']
    },
    {
      name: 'Honda',
      models: ['Civic', 'Accord', 'CR-V', 'HR-V', 'Jazz', 'Fit', 'Odyssey', 'Pilot', 'Insight', 'Prelude', 'S2000', 'Legend', 'Stream', 'FR-V', 'Element']
    },
    {
      name: 'Audi',
      models: ['A1', 'A3', 'A4', 'A5', 'A6', 'A7', 'A8', 'Q2', 'Q3', 'Q5', 'Q7', 'Q8', 'TT', 'S3', 'RS6']
    },
    {
      name: 'Volkswagen',
      models: ['Golf', 'Passat', 'Polo', 'Tiguan', 'Touareg', 'Jetta', 'Touran', 'Sharan', 'Arteon', 'T-Roc', 'T-Cross', 'Scirocco', 'Up!', 'Caddy', 'Amarok']
    }
  ];

  const mileageOptions = ['Any', '0-50,000', '50,001-100,000', '100,001-150,000', '150,001-200,000', '200,001+'];
  const yearOptions = ['Any', '2024', '2023', '2022', '2021', '2020', '2019', '2018', '2017', '2016', '2015', '2010-2014', '2005-2009', '2000-2004', 'Before 2000'];
  const priceOptions = ['Any', 'Up to €5,000', 'Up to €10,000', 'Up to €15,000', 'Up to €20,000', 'Up to €30,000', 'Up to €50,000', '€50,000+'];

  const handleBrandChange = (e) => {
    setSelectedBrand(e.target.value);
    setSelectedModel(''); // Reset model when brand changes
  };
  const handleModelChange = (e) => setSelectedModel(e.target.value);
  const handleMileageChange = (e) => setSelectedMileage(e.target.value);
  const handleYearChange = (e) => setSelectedYear(e.target.value);
  const handlePriceChange = (e) => setSelectedPrice(e.target.value);

  const handleHeroSearch = (e) => {
    e.preventDefault();
    if (!selectedBrand || !selectedModel) return; // Require brand and model
    const params = new URLSearchParams();
    params.append('make', selectedBrand);
    params.append('model', selectedModel);
    if (selectedMileage !== 'Any') params.append('mileage', selectedMileage);
    if (selectedYear !== 'Any') params.append('year', selectedYear);
    if (selectedPrice !== 'Any') params.append('price', selectedPrice);
    navigate(`/listings?${params.toString()}`);
  };

  // Build a map from brand name (lowercase, no spaces or dashes) to logo for robust lookup
  const brandLogoMap = {};
  brandLogos.forEach(({ name, logo }) => {
    brandLogoMap[name.toLowerCase().replace(/[-\s]/g, '')] = logo;
  });
  const fallbackLogo = 'https://via.placeholder.com/80x80?text=Logo';

  return (
    <div className="home-container">
      {/* Hero Section - Car Focused */}
      <section className="hero-section">
        <div className="hero-search-container">
          <h1 className="hero-title">Drive Something Your Neighbors Will Google</h1>
          <form className="hero-search-form" onSubmit={handleHeroSearch}>
            <div className="hero-search-row">
              <select value={selectedBrand} onChange={handleBrandChange} required className="hero-select">
                <option value="" disabled>Brand</option>
                {brandOptions.map(brand => (
                  <option key={brand.name} value={brand.name}>{brand.name}</option>
                ))}
              </select>
              <select value={selectedModel} onChange={handleModelChange} required className="hero-select" disabled={!selectedBrand}>
                <option value="" disabled>Model</option>
                {selectedBrand && brandOptions.find(b => b.name === selectedBrand)?.models.map(model => (
                  <option key={model} value={model}>{model}</option>
                ))}
              </select>
              <select value={selectedMileage} onChange={handleMileageChange} className="hero-select">
                {mileageOptions.map(opt => (
                  <option key={opt} value={opt}>{opt === 'Any' ? 'Mileage' : opt}</option>
                ))}
              </select>
            </div>
            <div className="hero-search-row">
              <select value={selectedYear} onChange={handleYearChange} className="hero-select">
                {yearOptions.map(opt => (
                  <option key={opt} value={opt}>{opt === 'Any' ? 'Year from' : opt}</option>
                ))}
              </select>
              <select value={selectedPrice} onChange={handlePriceChange} className="hero-select">
                {priceOptions.map(opt => (
                  <option key={opt} value={opt}>{opt === 'Any' ? 'Price up to' : opt}</option>
                ))}
              </select>
              <button type="submit" className="hero-search-btn">Search</button>
            </div>
            <div className="hero-search-details">
              <button type="button" className="hero-search-details-link" onClick={handleAdvancedSearch}>Advanced Search &gt;</button>
            </div>
          </form>
        </div>
      </section>

      {/* Featured Brands Section */}
      <section id="brands" className="listings-section">
        <div className="section-header">
          <h2 className="section-title">Featured Brands</h2>
          <div className="section-title-with-nav">
            <button ref={brandsPrevRef} className="swiper-nav-btn" aria-label="Previous brands">&#8592;</button>
            <button ref={brandsNextRef} className="swiper-nav-btn" aria-label="Next brands">&#8594;</button>
          </div>
        </div>

        <div className="swiper-container">
          <Swiper
            modules={[Navigation, Autoplay]}
            spaceBetween={20}
            slidesPerView={1}
            navigation={{
              prevEl: brandsPrevRef.current,
              nextEl: brandsNextRef.current,
            }}
            onInit={swiper => {
              swiper.params.navigation.prevEl = brandsPrevRef.current;
              swiper.params.navigation.nextEl = brandsNextRef.current;
              swiper.navigation.init();
              swiper.navigation.update();
            }}
            loop={true}
            autoplay={{
              delay: 4000,
              disableOnInteraction: false,
            }}
            breakpoints={{
              640: {
                slidesPerView: 2,
              },
              1024: {
                slidesPerView: 3,
              },
            }}
          >
            {brandOptions.map((brand) => {
              // Robust logo lookup
              const logoKey = brand.name.toLowerCase().replace(/[-\s]/g, '');
              const logo = brandLogoMap[logoKey] || fallbackLogo;
              if (logo === fallbackLogo) {
                console.warn('Missing logo for brand:', brand.name, 'Expected key:', logoKey);
              }
              return (
                <SwiperSlide key={brand.name}>
                  <div
                    className="simplified-card"
                    onClick={() => handleBrandClick(brand.name)}
                  >
                    <div className="car-image" style={{ background: '#fff', padding: '8px', borderRadius: '12px', display: 'flex', alignItems: 'center', justifyContent: 'center', height: '96px', boxShadow: '0 1px 4px rgba(0,0,0,0.04)' }}>
                      <img src={logo} alt={brand.name + ' logo'} style={{ width: '80px', height: '80px', objectFit: 'contain', background: 'transparent', borderRadius: '8px', display: 'block' }} />
                    </div>
                    <div className="simplified-info">
                      <h3 className="car-title">{brand.name}</h3>
                    </div>
                  </div>
                </SwiperSlide>
              );
            })}
          </Swiper>
        </div>
      </section>

      {/* --- NEW: Info Section (TrueCar style) --- */}
      <section className="info-section">
        <h2 className="info-title">Why CarSeek?</h2>
        <div className="info-features">
          <div className="info-feature">
            <div className="info-icon info-icon-blue">
              <svg width="64" height="64" fill="none" xmlns="http://www.w3.org/2000/svg"><rect width="64" height="64" rx="32" fill="#0ff" fillOpacity="0.08"/><path d="M20 44c0-2.2 4.5-4 10-4s10 1.8 10 4" stroke="#0ff" strokeWidth="2" strokeLinecap="round"/><rect x="24" y="24" width="16" height="10" rx="2" stroke="#0ff" strokeWidth="2"/><path d="M28 34v2m8-2v2" stroke="#0ff" strokeWidth="2" strokeLinecap="round"/></svg>
            </div>
            <h3>Transparent Pricing, No Surprises</h3>
            <p>Know exactly what you’ll pay — honest and straightforward.</p>
          </div>
          <div className="info-feature">
            <div className="info-icon info-icon-blue">
              <svg width="64" height="64" fill="none" xmlns="http://www.w3.org/2000/svg"><rect width="64" height="64" rx="32" fill="#0ff" fillOpacity="0.08"/><path d="M32 44c5.5 0 10-1.8 10-4s-4.5-4-10-4-10 1.8-10 4 4.5 4 10 4z" stroke="#0ff" strokeWidth="2"/><rect x="28" y="24" width="8" height="10" rx="2" stroke="#0ff" strokeWidth="2"/><path d="M32 34v2" stroke="#0ff" strokeWidth="2" strokeLinecap="round"/></svg>
            </div>
            <h3>Efficient Car Search, Designed for You</h3>
            <p>Powerful tools that save you time and get you behind the wheel faster.</p>
          </div>
          <div className="info-feature">
            <div className="info-icon info-icon-orange">
              <svg width="64" height="64" fill="none" xmlns="http://www.w3.org/2000/svg"><rect width="64" height="64" rx="32" fill="#ff9800" fillOpacity="0.08"/><path d="M44 44c0-2.2-4.5-4-10-4s-10 1.8-10 4" stroke="#ff9800" strokeWidth="2" strokeLinecap="round"/><rect x="24" y="24" width="16" height="10" rx="2" stroke="#ff9800" strokeWidth="2"/><path d="M28 34v2m8-2v2" stroke="#ff9800" strokeWidth="2" strokeLinecap="round"/></svg>
            </div>
            <h3>Shop on Your Terms. No Pushy Salespeople Allowed.</h3>
            <p>Swipe, click, decide — zero awkward conversations guaranteed.</p>
          </div>
        </div>
        <div className="info-signup-row">
          {!isAuthenticated && (
            <button className="info-signup-btn" onClick={() => navigate('/register')}>Sign up</button>
          )}
        </div>
      </section>

      {/* Car Listings Section */}
      <section id="listings" className="listings-section">
        <div className="section-header">
          <h2 className="section-title">Featured Cars</h2>
          <div className="section-title-with-nav">
            <button ref={carsPrevRef} className="swiper-nav-btn" aria-label="Previous cars">&#8592;</button>
            <button ref={carsNextRef} className="swiper-nav-btn" aria-label="Next cars">&#8594;</button>
          </div>
          {isAuthenticated && user?.role === 'Dealer' && (
            <a href="/dashboard" className="add-car-button">+ List Your Car</a>
          )}
        </div>

        {error && (
          <div className="error-message">
            {error}
          </div>
        )}

        {carListings.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">🚗</div>
            <h3>New Cars Coming Soon!</h3>
            <p>We're working with dealers to bring you the best selection of vehicles.</p>
            {!isAuthenticated && (
              <p className="empty-subtext">
                Are you a dealer? <a href="/register" className="link">Join CarSeek</a> to list your cars!
              </p>
            )}
          </div>
        ) : (
          <div className="swiper-container">
            <Swiper
              modules={[Navigation, Autoplay]} // Remove Pagination
              spaceBetween={20}
              slidesPerView={1}
              navigation={{
                prevEl: carsPrevRef.current,
                nextEl: carsNextRef.current,
              }}
              onInit={swiper => {
                swiper.params.navigation.prevEl = carsPrevRef.current;
                swiper.params.navigation.nextEl = carsNextRef.current;
                swiper.navigation.init();
                swiper.navigation.update();
              }}
              loop={true} // Enable continuous loop
              autoplay={{
                delay: 3000,
                disableOnInteraction: false,
              }}
              breakpoints={{
                640: {
                  slidesPerView: 2,
                },
                1024: {
                  slidesPerView: 3,
                },
              }}
            >
              {carListings.map((car) => (
                <SwiperSlide key={car.id}>
                  <div
                    className="simplified-card"
                    onClick={() => handleViewDetails(car.id)}
                  >
                    <div className="car-image">
                      {car.primaryImageUrl || car.images?.[0]?.imageUrl ? (
                        <img
                          src={car.primaryImageUrl || car.images[0].imageUrl}
                          alt={car.images?.[0]?.altText || `${car.year} ${car.make} ${car.model}`}
                          style={{width: '100%', height: '100%', objectFit: 'cover', borderRadius: '8px'}}
                        />
                      ) : (
                        <div className="image-placeholder">📷</div>
                      )}
                    </div>
                    <div className="simplified-info">
                      <h3 className="car-title">
                        {car.year} {car.make} {car.model}
                      </h3>
                      <p className="car-price" style={{ color: '#2563eb' }}>
                        ${car.price?.toLocaleString()}
                      </p>
                    </div>
                  </div>
                </SwiperSlide>
              ))}
            </Swiper>
          </div>
        )}
      </section>
    </div>
  );
};

export default Home;
