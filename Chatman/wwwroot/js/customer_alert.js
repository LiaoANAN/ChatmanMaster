(function ($) {
    // Alert counter for unique IDs
    let alertCounter = 0;

    // Alert class with configuration
    class CustomAlert {
        constructor(message, type = 'success', duration = 3000) {
            this.id = ++alertCounter;
            this.message = message;
            this.type = type;
            this.duration = duration;
            this.element = null;
        }

        getIcon() {
            switch (this.type) {
                case 'success':
                    return 'fa-check-circle';
                case 'error':
                    return 'fa-exclamation-circle';
                case 'warning':
                    return 'fa-exclamation-triangle';
                case 'info':
                    return 'fa-info-circle';
                default:
                    return 'fa-bell';
            }
        }

        create() {
            // Clone template
            const template = $('#alert-template').html();
            this.element = $(template);

            // Set unique ID
            this.element.attr('id', `alert-${this.id}`);

            // Add type class
            this.element.addClass(this.type);

            // Set message
            this.element.find('.alert-message').text(this.message);

            // Set icon
            this.element.find('.alert-icon').removeClass('fa-check-circle').addClass(this.getIcon());

            // Add to container
            $('#custom-alert-container').append(this.element);

            // Setup close button
            this.element.find('.alert-close').on('click', () => this.close());

            // Auto close after duration
            if (this.duration) {
                setTimeout(() => this.close(), this.duration);
            }

            return this;
        }

        close() {
            if (!this.element) return;

            this.element.addClass('hiding');
            setTimeout(() => {
                this.element.remove();
            }, 300);
        }
    }

    // Global function to show alert
    window.showAlert = function (message, type = 'success', duration = 3000) {
        return new CustomAlert(message, type, duration).create();
    };
})(jQuery);