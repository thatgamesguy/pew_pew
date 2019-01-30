using UnityEngine;
using System.Collections;
using GameCore;

namespace PE2D
{
    /// <summary>
    /// Main workhorse for the custom particles.
    /// Updates particles state (colour, position, velocity etc), handles interaction with effectors, and applys any screen constraints.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class CustomParticle : MonoBehaviour
    {

        #region individual particle configuration
        /// <summary>
        /// Update sprites alpha based on velovity.
        /// </summary>
        public bool shouldUpdateAlpha = true;

        /// <summary>
        /// Update sprites scale based on velicoty.
        /// </summary>
        public bool shouldUpdateScale = true;

        public bool ignoreEffectors
        {
            set
            {
                _state.ignoreEffectors = value;
            }
        }

        public bool canBeCollectedByPlayer
        {
            set
            {
                _state.canBeCollectedByPlayer = value;
            }
        }
        #endregion


        /// <summary>
        /// Set the state of the particles. Also resets particles properties.
        /// </summary>
        /// <value>The state.</value>
        public ParticleBuilder state
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                _shouldUpdate = true;
                _externalVelocityUpdate = false;
                _alphaThreshold = (_state.customAlphaThreshold.HasValue) ? _state.customAlphaThreshold.Value : ALPHA_THRESHOLD;
                _velocityThreshold = (_state.customVelocityThreshold.HasValue) ? _state.customVelocityThreshold.Value : MOVEMENT_THRESHOLD;
            }
        }

        /// <summary>
        /// Maximum duration of particles life. Life may be shorter dependent on velocity.
        /// </summary>
        /// <value>The duration.</value>
        public float duration { get; set; }

        /// <summary>
        /// Range (0, 1). 0 = time to remove from scene, 1 = just spawned.
        /// </summary>
        /// <value>The percent life.</value>
        public float percentLife { get; set; }

        /// <summary>
        /// Gets the sprite renderer.
        /// </summary>
        /// <value>The sprite renderer.</value>
        public SpriteRenderer spriteRenderer { get { return _renderer; } }

        #region static variables
        private static readonly float NORMAL_SCALE = 2f;
        private static readonly float MOVEMENT_THRESHOLD = 0.000000001f;
        private static readonly float ALPHA_THRESHOLD = 0.000005f;
        private static readonly int ALPHA_MULTIPLIER = 255;
        private static readonly float IGNORE_EFFECTOR_TIME = 0.25f;
        private static readonly float IGNORE_PLAYER_DISTANCE_TIME = 0.1f;
        private static Transform PLAYER;
        private static PointPopUpUI POINTS_FACTORY;
        private static ParticleEffector[] _effectors;
        #endregion

        private ParticleBuilder _state;
        private SpriteRenderer _renderer;
        private bool _shouldUpdate;
        private Vector2 _velocity;
        private bool _externalVelocityUpdate;
        private float _alphaThreshold;
        private float _velocityThreshold;
        private float _ignoreEffectorTime;
        private float _ignorePlayerTime;

        void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();

            if (PLAYER == null)
            {
                PLAYER = GameObject.FindGameObjectWithTag("Player").transform;
            }

            if (POINTS_FACTORY == null)
            {
                POINTS_FACTORY = GameObject.FindGameObjectWithTag("PointsUI").GetComponent<PointPopUpUI>();
            }

        }

        void OnEnable()
        {
            _ignoreEffectorTime = 0f;
            _ignorePlayerTime = 0f;
        }

        public void Pause()
        {
            _shouldUpdate = false;
        }

        public void Resume()
        {
            _shouldUpdate = true;
        }

        void Update()
        {
            if (!_shouldUpdate)
                return;

            if (_state.canBeCollectedByPlayer)
            {
                _ignorePlayerTime += Time.deltaTime;

                if (_ignorePlayerTime > IGNORE_PLAYER_DISTANCE_TIME &&
                    Vector2.Distance(transform.position, PLAYER.position) < 0.5f)
                {
                    if (POINTS_FACTORY != null)
                    {
                        POINTS_FACTORY.ShowAtPosition(transform.position);
                    }
                    percentLife = 0f;
                }

            }


            if (!_externalVelocityUpdate)
            {
                _velocity = _state.velocity;
            }

            transform.position += (Vector3)_velocity * Time.deltaTime * 50f;

            Vector2 dir = _velocity;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            float speed = _velocity.magnitude;
            float? alpha = UpdateAlpha(speed);
            alpha = UpdateScale(speed, alpha);

            if (!_state.ignoreEffectors)
            {
                _ignoreEffectorTime += Time.deltaTime;

                if (_state.ignoreInitialEffectorTime
                    || _ignoreEffectorTime > IGNORE_EFFECTOR_TIME)
                {
                    UpdateEffectorForce();
                }
            }

            if (speed < _velocityThreshold)
            {
                _velocity = Vector2.zero;
                _shouldUpdate = false;

                if (_state.removeWhenVelocityReachesThreshold.HasValue && _state.removeWhenVelocityReachesThreshold.Value)
                {
                    percentLife = 0f;
                }

            }
            else if (alpha < _alphaThreshold)
            {
                _shouldUpdate = false;

                if (_state.removeWhenAlphaReachesThreshold.HasValue && _state.removeWhenAlphaReachesThreshold.Value)
                {
                    percentLife = 0f;
                }

            }
            else
            {
                _velocity *= _state.velocityDampModifier;
            }

            _state.velocity = _velocity;

            _externalVelocityUpdate = false;

        }

        void OnBecameInvisible()
        {
            if (_state.wrapAroundType == WrapAroundType.None || !_shouldUpdate)
            {
                percentLife = 0f;
                return;
            }

            var cam = Camera.main;

            if (cam == null)
                return;

            var viewportPos = Camera.main.WorldToViewportPoint(transform.position);

            if (_state.wrapAroundType == WrapAroundType.Constrain)
            {
                if (viewportPos.x > 1)
                {
                    _velocity.x = -Mathf.Abs(_velocity.x);
                }
                else if (viewportPos.x < 0)
                {
                    _velocity.x = Mathf.Abs(_velocity.x);
                }
                else if (viewportPos.y < 0)
                {
                    _velocity.y = Mathf.Abs(_velocity.y);
                }
                else if (viewportPos.y > 1)
                {
                    _velocity.y = -Mathf.Abs(_velocity.y);
                }
                _externalVelocityUpdate = true;
            }
            else if (_state.wrapAroundType == WrapAroundType.WrapAround)
            {
                var newPos = transform.position;
                if (viewportPos.x > 1)
                {
                    newPos.x = Camera.main.ViewportToWorldPoint(Vector3.zero).x;
                }

                if (viewportPos.x < 0)
                {
                    newPos.x = Camera.main.ViewportToWorldPoint(Vector3.one).x;
                }

                if (viewportPos.y > 1)
                {
                    newPos.y = Camera.main.ViewportToWorldPoint(Vector3.zero).y;
                }

                if (viewportPos.y < 0)
                {
                    newPos.y = Camera.main.ViewportToWorldPoint(Vector3.one).y;
                }

                transform.position = newPos;
            }
        }

        /// <summary>
        /// Finds all effectors in scene. Static reference should only be called once for all particles on effector change.
        /// </summary>
        public static void UpdateEffectorList()
        {
            _effectors = GameObject.FindObjectsOfType<ParticleEffector>();
        }

        private void UpdateEffectorForce()
        {
            // Ignore attract attached to player if any other effectors in scene.
            bool ignoreAttract = false;

            foreach (var effector in _effectors)
            {
                if (effector == null || effector.force == 0f)
                {
                    continue;
                }

                if (effector.effectorType == EffectorType.BlackHole
                    || effector.effectorType == EffectorType.Repel)
                {
                    ignoreAttract = true;
                    break;
                }
            }


            foreach (var effector in _effectors)
            {

                if (effector == null || effector.force == 0f
                    || (ignoreAttract && effector.effectorType == EffectorType.Attraction))
                {
                    continue;
                }

                var heading = effector.transform.position - transform.position;

                if (heading.sqrMagnitude > effector.distance * effector.distance)
                {
                    continue;
                }

                if (effector.effectorType == EffectorType.Attraction)
                {
                    float distance = heading.magnitude;
                    var n = heading / distance;

                    _velocity += (Vector2)(10000 * n / (distance * distance + 10000)) * effector.force;
                }
                else if (effector.effectorType == EffectorType.Repel)
                {
                    float distance = heading.magnitude;
                    var n = heading / distance;

                    _velocity -= (Vector2)(10000 * n / (distance * distance + 10000)) * effector.force;

                }
                else if (effector.effectorType == EffectorType.BlackHole)
                {
                    float distance = heading.magnitude;
                    var n = heading / distance;

                    _velocity += (Vector2)(10000 * n / (distance * distance + 10000)) * effector.force;

                    if (distance < effector.rotateDistance)
                    {
                        _velocity += 45 * new Vector2(n.y, -n.x) / (distance + 100) * effector.force;
                    }
                }
            }
        }

        private float? UpdateAlpha(float speed)
        {
            if (shouldUpdateAlpha)
            {
                float alpha = CalculateAlpha(speed);

                var colour = _renderer.color;
                _renderer.color = new Color(colour.r, colour.g, colour.b, ALPHA_MULTIPLIER * alpha);

                return alpha;
            }

            return null;
        }

        private float? UpdateScale(float speed, float? alpha)
        {
            if (shouldUpdateScale)
            {
                float alphaInst = (alpha.HasValue) ? alpha.Value : CalculateAlpha(speed);

                var curScale = transform.localScale;
                float scaleX = _state.lengthMultiplier * Mathf.Min(Mathf.Min(1f, 0.2f * speed + 0.1f * NORMAL_SCALE), alphaInst);
                scaleX = Mathf.Clamp(scaleX,
                                              _state.minLengthClamp.HasValue ? _state.minLengthClamp.Value : scaleX,
                                              _state.maxLengthClamp.HasValue ? _state.maxLengthClamp.Value : scaleX);


                transform.localScale = new Vector3(scaleX, curScale.y, curScale.z);

                return alphaInst;
            }

            return alpha;
        }

        private float CalculateAlpha(float speed)
        {
            float alpha = Mathf.Min(1f, Mathf.Min(percentLife * 2 * NORMAL_SCALE, speed * NORMAL_SCALE));
            alpha *= alpha;
            return alpha;
        }
    }
}
