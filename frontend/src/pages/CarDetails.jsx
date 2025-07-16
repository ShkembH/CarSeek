import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { apiService } from '../services/api';
import '../styles/pages/CarDetails.css';
import { useAuth } from '../context/AuthContext';
import { Swiper, SwiperSlide } from 'swiper/react';
import { Navigation, Autoplay } from 'swiper/modules';
import 'swiper/css';
import 'swiper/css/navigation';
import ChatModal from '../components/common/ChatModal';

function CarDetails() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [car, setCar] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [activeImage, setActiveImage] = useState(0);
  const { user, isAuthenticated } = useAuth();
  const [isSaved, setIsSaved] = useState(false);
  const [similarCars, setSimilarCars] = useState([]);
  const similarPrevRef = React.useRef(null);
  const similarNextRef = React.useRef(null);
  const [showChat, setShowChat] = useState(false);

  useEffect(() => {
    const fetchCarDetails = async () => {
      try {
        setLoading(true);
        const response = await apiService.getCarListing(id);
        setCar(response); // Use the response directly
      } catch (err) {
        setError('Failed to load car details');
        console.error('Error fetching car details:', err);
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      fetchCarDetails();
    }
  }, [id]);

  useEffect(() => {
    if (isAuthenticated && car) {
      apiService.getSavedListings().then(saved => {
        setIsSaved(saved.some(l => l.id === car.id));
      });
    }
  }, [isAuthenticated, car]);

  useEffect(() => {
    if (car && car.make) {
      apiService.getCarListings().then(res => {
        if (res && res.items) {
          setSimilarCars(res.items.filter(c => c.make === car.make && c.id !== car.id));
        }
      });
    }
  }, [car]);

  const handleSave = async () => {
    if (!isAuthenticated) {
      alert('Please log in to save listings.');
      return;
    }

    try {
      if (isSaved) {
        await apiService.unsaveListing(car.id);
        setIsSaved(false);
        console.log('Listing unsaved successfully');
      } else {
        await apiService.saveListing(car.id);
        setIsSaved(true);
        console.log('Listing saved successfully');
      }
    } catch (error) {
      console.error('Error saving/unsaving listing:', error);
      alert('Failed to save/unsave listing. Please try again.');
    }
  };

  const handleImageClick = (index) => {
    setActiveImage(index);
  };



  const scrollToContact = () => {
    const contactSection = document.getElementById('contact-section');
    if (contactSection) {
      contactSection.scrollIntoView({ behavior: 'smooth' });
    }
  };

  // Parse the features from the car listing
  const getSelectedFeatures = () => {
    if (!car?.features) return [];

    try {
      const parsedFeatures = JSON.parse(car.features);
      return parsedFeatures;
    } catch (error) {
      console.error('Error parsing features:', error);
      return [];
    }
  };

  if (loading) {
    return (
      <div className="car-details-container">
        <div className="text-center p-8">Loading car details...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="car-details-container">
        <div className="text-center text-red-600 p-8">{error}</div>
        <button
          onClick={() => navigate('/listings')}
          className="car-details-back-button"
        >
          ← Back to Listings
        </button>
      </div>
    );
  }

  if (!car) {
    return (
      <div className="car-details-container">
        <div className="text-center p-8">Car not found</div>
        <button
          onClick={() => navigate('/listings')}
          className="car-details-back-button"
        >
          ← Back to Listings
        </button>
      </div>
    );
  }

  console.log("car object:", car);

  // Placeholder images if car doesn't have any
  const carImages = car.images && car.images.length > 0
    ? car.images
    : [{ imageUrl: 'https://picsum.photos/800/600?random=1' }];

  const selectedFeatures = getSelectedFeatures();

  // Debug logs
  console.log('showChat:', showChat, 'car:', car);

  const chatPanel = showChat && car && (
    <div className="car-details-chat-panel">
      <ChatModal
        ownerId={car.userId}
        listingId={car.id}
        onClose={() => setShowChat(false)}
        currentUserId={user.id}
        token={localStorage.getItem('token')}
        otherUserName={car.dealership ? car.dealership.name : car.ownerName || 'Private Seller'}
      />
    </div>
  );

  return (
    <div className="car-details-container">
      {chatPanel}
      <button
        onClick={() => navigate('/listings')}
        className="car-details-back-button"
      >
        ← Back to Listings
      </button>

      <div className="car-details-main">
        {/* Left side - Image gallery */}
        <div className="car-details-gallery">
          <img
            src={carImages[activeImage].imageUrl}
            alt={car.title}
            className="car-details-main-image"
          />

          {carImages.length > 1 && (
            <div className="car-details-thumbnails">
              {carImages.map((image, index) => (
                <img
                  key={index}
                  src={image.imageUrl}
                  alt={`${car.title} - view ${index + 1}`}
                  className={`car-details-thumbnail ${index === activeImage ? 'active' : ''}`}
                  onClick={() => handleImageClick(index)}
                />
              ))}
            </div>
          )}
        </div>

        {/* Right side - Car info */}
        <div className="car-details-info">
          <h1 className="car-details-title">{car.title}</h1>
          <p className="car-details-subtitle">{car.year} {car.make} {car.model}</p>

          <div className="car-details-price">€ {car.price?.toLocaleString()}</div>

          <div className="car-details-specs">
            <div className="car-details-spec-item">
              <span className="car-details-spec-label">Mileage</span>
              <span className="car-details-spec-value">{car.mileage?.toLocaleString()} km</span>
            </div>
            <div className="car-details-spec-item">
              <span className="car-details-spec-label">Year</span>
              <span className="car-details-spec-value">{car.year}</span>
            </div>
            <div className="car-details-spec-item">
              <span className="car-details-spec-label">Fuel Type</span>
              <span className="car-details-spec-value">{car.fuelType || 'Not specified'}</span>
            </div>
            <div className="car-details-spec-item">
              <span className="car-details-spec-label">Transmission</span>
              <span className="car-details-spec-value">{car.transmission || 'Not specified'}</span>
            </div>
            <div className="car-details-spec-item">
              <span className="car-details-spec-label">Color</span>
              <span className="car-details-spec-value">{car.color || 'Not specified'}</span>
            </div>
          </div>

          <div className="car-details-actions">
            <button
              className="car-details-action-button listings-btn"
              onClick={scrollToContact}
            >
              Contact Seller
            </button>
            {isAuthenticated && (
              <button className={`listings-btn ms-2 ${isSaved ? 'unsave' : ''}`} onClick={handleSave}>
                {isSaved ? 'Unsave Listing' : 'Save Listing'}
              </button>
            )}
          </div>

          <div className="car-details-seller">
            <h3 className="car-details-seller-title">Seller</h3>
            <div className="car-details-seller-info">
              <div className="car-details-seller-icon">👤</div>
              <div>
                <div className="car-details-seller-name">
                  {car.dealership ? car.dealership.name : car.ownerName || 'Private Seller'}
                </div>
                <div>{car.dealership ? 'Dealership' : 'Private Seller'}</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="car-details-description">
        <h2 className="car-details-description-title">Description</h2>
        <p className="car-details-description-content">{car.description}</p>
      </div>

      {/* Vehicle Features Section */}
      {selectedFeatures.length > 0 && (
        <div className="car-details-specifications">
          <h2 className="car-details-specifications-title">Vehicle Features</h2>
          <div className="car-details-features-grid">
            {selectedFeatures.map(feature => (
              <div key={feature} className="car-details-feature-item">
                <span className="car-details-feature-icon">✓</span>
                <span className="car-details-feature-text">{feature}</span>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Contact Information Section */}
      <div id="contact-section" className="car-details-contact">
        <h2 className="car-details-contact-title">Contact Information</h2>
        <div className="car-details-contact-content">
          <div className="car-details-contact-seller">
            <h3 className="car-details-contact-seller-title">
              {car.dealership ? car.dealership.name : car.ownerName || 'Private Seller'}
            </h3>
            <p className="car-details-contact-seller-type">
              {car.dealership ? '🏢 Dealership' : '👤 Private Seller'}
            </p>

            {car.dealership && (
              <div className="car-details-contact-dealership-info">
                {car.dealership.phoneNumber && (
                  <div className="car-details-contact-item">
                    <span className="car-details-contact-label">📞 Phone:</span>
                    <span className="car-details-contact-value">{car.dealership.phoneNumber}</span>
                  </div>
                )}
                {car.dealership.website && (
                  <div className="car-details-contact-item">
                    <span className="car-details-contact-label">🌐 Website:</span>
                    <a href={car.dealership.website} target="_blank" rel="noopener noreferrer" className="car-details-contact-link">
                      {car.dealership.website}
                    </a>
                  </div>
                )}
                {car.dealership.address && (
                  <div className="car-details-contact-item">
                    <span className="car-details-contact-label">📍 Address:</span>
                    <span className="car-details-contact-value">
                      {car.dealership.address.street}, {car.dealership.address.city}, {car.dealership.address.state} {car.dealership.address.postalCode}, {car.dealership.address.country}
                    </span>
                  </div>
                )}
              </div>
            )}

            {car.ownerEmail && (
              <div className="car-details-contact-item">
                <span className="car-details-contact-label">📧 Email:</span>
                <a href={`mailto:${car.ownerEmail}`} className="car-details-contact-link">
                  {car.ownerEmail}
                </a>
              </div>
            )}
            {/* Show owner's phone number for private sellers */}
            {!car.dealership && car.ownerPhone && (
              <div className="car-details-contact-item">
                <span className="car-details-contact-label">📞 Phone:</span>
                <span className="car-details-contact-value">{car.ownerPhone}</span>
              </div>
            )}
          </div>

          <div className="car-details-contact-actions">
            {isAuthenticated && user?.id !== car.userId && (
              <button
                type="button"
                className="listings-btn car-details-contact-button-large"
                onClick={() => setShowChat(true)}
              >
                💬 Chat with Owner
              </button>
            )}
          </div>
        </div>
      </div>

      {/* Similar Cars Section */}
      {similarCars.length > 0 && (
        <section className="listings-section" style={{marginTop: '3rem'}}>
          <div className="section-header">
            <h2 className="section-title">Similar Cars</h2>
            <div className="section-title-with-nav">
              <button ref={similarPrevRef} className="swiper-nav-btn" aria-label="Previous similar cars">&#8592;</button>
              <button ref={similarNextRef} className="swiper-nav-btn" aria-label="Next similar cars">&#8594;</button>
            </div>
          </div>
          <div className="swiper-container">
            <Swiper
              modules={[Navigation, Autoplay]}
              spaceBetween={20}
              slidesPerView={1}
              navigation={{
                prevEl: similarPrevRef.current,
                nextEl: similarNextRef.current,
              }}
              onInit={swiper => {
                swiper.params.navigation.prevEl = similarPrevRef.current;
                swiper.params.navigation.nextEl = similarNextRef.current;
                swiper.navigation.init();
                swiper.navigation.update();
              }}
              loop={true}
              autoplay={{
                delay: 3500,
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
              {similarCars.map(simCar => (
                <SwiperSlide key={simCar.id}>
                  <div
                    className="simplified-card"
                    onClick={() => navigate(`/listings/${simCar.id}`)}
                  >
                    <div className="car-image">
                      <img
                        src={simCar.primaryImageUrl || simCar.images?.[0]?.imageUrl || 'https://picsum.photos/300/200?random=1'}
                        alt={`${simCar.year} ${simCar.make} ${simCar.model}`}
                        style={{width: '100%', height: '100%', objectFit: 'cover', borderRadius: '8px'}}
                      />
                    </div>
                    <div className="simplified-info">
                      <h3 className="car-title">
                        {simCar.year} {simCar.make} {simCar.model}
                      </h3>
                      <p className="car-price" style={{ color: '#2563eb' }}>
                        €{simCar.price?.toLocaleString()}
                      </p>
                    </div>
                  </div>
                </SwiperSlide>
              ))}
            </Swiper>
          </div>
        </section>
      )}
      <style>{`
        .car-details-chat-panel {
          position: fixed;
          top: 0;
          right: 0;
          height: 100vh;
          width: 400px;
          max-width: 100vw;
          background: #fff;
          box-shadow: -2px 0 16px rgba(0,0,0,0.08);
          z-index: 1200;
          display: flex;
          flex-direction: column;
          border-left: 1px solid #eee;
        }
        @media (max-width: 600px) {
          .car-details-chat-panel {
            width: 100vw;
            left: 0;
            right: 0;
            border-left: none;
            border-top: 1px solid #eee;
            top: auto;
            bottom: 0;
            height: 60vh;
          }
        }
      `}</style>
    </div>
  );
}

export default CarDetails;
